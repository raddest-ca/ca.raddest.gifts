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
        var group = await _services.TableClient.GetGroupIfExistsAsync(id);
        if (group == null)
        {
            return BadRequest("group not found");
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
        public Guid UserId {get; set;}
        public string GroupPassword { get; set; }
    }

    [HttpPost("{groupId}/join")]
    public async Task<IActionResult> JoinGroup(Guid groupId, [FromBody] JoinGroupPayload payload)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("group not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(
            HttpContext.User,
            group,
            new JoinGroupAuthorizationRequirement
            {
                UserId = payload.UserId,
                UserProvidedPassword = payload.GroupPassword
            }
        );

        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        group.Members = group.Members.Append(payload.UserId).ToArray();
        await _services.TableClient.UpdateEntityAsync(group.Entity, group.Entity.ETag);
        return Ok();
    }

    [HttpDelete("{groupId:guid}/member/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid userId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("group not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(
            HttpContext.User,
            group,
            new ModifyGroupUserAuthorizationRequirement
            {
                UserId = userId,
                Requirement = CrudRequirements.Delete,
            }
        );

        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        group.Members = group.Members.Where(x => x != userId).ToArray();
        group.Owners = group.Owners.Where(x => x != userId).ToArray();
        if (group.Owners.Length == 0)
        {
            return BadRequest("cannot remove last owner");
        }
        if (group.Members.Length == 0)
        {
            return BadRequest("cannot remove last member");
        }
        await _services.TableClient.UpdateEntityAsync(group.Entity, group.Entity.ETag);
        return Ok();
    }

    public class GroupMemberUpdatePayload
    {
        public bool IsOwner { get; set; }
    }
    [HttpPatch("{groupId:guid}/member/{userId:guid}")]
    public async Task<IActionResult> UpdateMember(Guid groupId, Guid userId, [FromBody] GroupMemberUpdatePayload payload)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("group not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(
            HttpContext.User,
            group,
            new ModifyGroupUserAuthorizationRequirement
            {
                UserId = userId,
                Requirement = CrudRequirements.Update,
            }
        );

        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        if (payload.IsOwner)
        {
            group.Owners = group.Owners.Append(userId).ToArray();
        }
        else
        {
            group.Owners = group.Owners.Where(x => x != userId).ToArray();
        }

        if (group.Owners.Length == 0)
        {
            return BadRequest("cannot remove last owner");
        }
        await _services.TableClient.UpdateEntityAsync(group.Entity, group.Entity.ETag);
        return Ok();
    }

    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid groupId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return NotFound();
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Delete);
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        await _services.TableClient.DeleteGroupAsync(group);
        return Ok();
    }
}