using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly Services<GroupController> _services;

    public GroupController(Services<GroupController> services)
    {
        _services = services;
    }


    public class GroupCreatePayload
    {
        public string GroupDisplayName { get; set; }
        public string GroupPassword { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] GroupCreatePayload payload)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            DisplayName = payload.GroupDisplayName,
            Password = payload.GroupPassword, // store password in plaintext for groups
            Members = new Guid[] { HttpContext.User.GetUserId()!.Value },
            Owners = new Guid[] { HttpContext.User.GetUserId()!.Value },
        };

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Create);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        await _services.TableClient.AddEntityAsync(group.Entity);
        return new JsonResult(group);
    }

    [HttpGet]
    public async Task<IActionResult> GetGroups()
    {
        var items = _services.TableClient.QueryAsync<GroupEntity>(filter: $"PartitionKey eq 'Group'").WhereAwait(async groupEntity =>
        {
            var group = new Group { Entity = groupEntity };
            var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
            return authResult.Succeeded;
        });
        return new JsonResult(await items.Select(x => new Group{Entity=x}).ToArrayAsync());
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var group = await _services.TableClient.GetGroupByIdAsync(id);
        if (group == null)
        {
            return NotFound();
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        return new JsonResult(group);
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

        // validate join request
        var auth = await _services.AuthService.AuthorizeAsync(
            HttpContext.User,
            group,
            new JoinGroupAuthorizationRequirement
            {
                UserProvidedPassword = payload.GroupPassword
            }
        );

        if (!auth.Succeeded)
        {
            return Problem(
                detail: string.Join(",", auth.Failure?.FailureReasons.Select(x => x.Message) ?? new string[0]),
                statusCode: StatusCodes.Status403Forbidden
            );
        }


        // add user to group
        group.Members = group.Members.Append(HttpContext.User.GetUserId()!.Value).ToArray();
        await _services.TableClient.UpdateEntityAsync(group.Entity, group.Entity.ETag);
        return Ok();
    }

    public class DeleteGroupPayload
    {
        public Guid GroupId { get; set; }
    }
    [HttpDelete]
    public async Task<IActionResult> DeleteGroup([FromBody] DeleteGroupPayload payload)
    {
        var group = await _services.TableClient.GetGroupByIdAsync(payload.GroupId);
        if (group == null)
        {
            return NotFound();
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Delete);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        await _services.TableClient.DeleteEntityAsync(group.Entity.PartitionKey, group.Entity.RowKey);
        return Ok();
    }
}