using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly Services<GroupsController> _services;

    public GroupsController(Services<GroupsController> services)
    {
        _services = services;
    }


    public class GroupCreatePayload
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] GroupCreatePayload payload)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            DisplayName = payload.Name,
            Password = BC.HashPassword(payload.Password),
            Members = new Guid[] { HttpContext.User.GetUserId()!.Value },
        };

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Create);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        await _services.TableClient.AddEntityAsync(group);
        return new JsonResult(group);
    }

    [HttpGet]
    public async Task<IActionResult> GetGroups()
    {
        var items = _services.TableClient.QueryAsync<Group>(filter: $"PartitionKey eq 'group'").WhereAwait(async group =>
        {
            var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
            return authResult.Succeeded;
        });
        return new JsonResult(await items.ToArrayAsync());
    }


    public class JoinGroupPayload
    {
        public Guid GroupId { get; set; }
        public string GroupPassword { get; set; }
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinGroup([FromBody] JoinGroupPayload payload)
    {
        // lookup group
        var group = await _services.TableClient.GetGroupByIdAsync(payload.GroupId);
        if (group == null)
        {
            return NotFound();
        }

        // validate password
        var req = new PasswordRequirement
        {
            UserProvidedPassword = payload.GroupPassword
        };
        if (!(await _services.AuthService.AuthorizeAsync(HttpContext.User, group, req)).Succeeded)
        {
            return Forbid();
        }

        var userId = HttpContext.User.GetUserId();
        if (userId == null)
        {
            return Forbid();
        }

        // add user to group
        group.Members = group.Members.Append(userId.Value).ToArray();
        await _services.TableClient.UpdateEntityAsync(group, group.ETag);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var group = await _services.TableClient.GetGroupByIdAsync(id);
        if (group == null)
        {
            return NotFound();
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Delete);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        await _services.TableClient.DeleteEntityAsync(group.PartitionKey, group.RowKey);
        return Ok();
    }
}