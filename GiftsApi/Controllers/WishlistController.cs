using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:guid}/[controller]")]
public class GroupListController : ControllerBase
{
    private readonly Services<GroupListController> _services;

    public GroupListController(Services<GroupListController> services)
    {
        _services = services;
    }


    public class CreateWishlistPayload
    {
        public string DisplayName { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> Create(Guid groupId, [FromBody] CreateWishlistPayload payload)
    {
        var list = new Wishlist
        {
            Id = Guid.NewGuid(),
            DisplayName = payload.DisplayName,
            Owners = new Guid[] { HttpContext.User.GetUserId()!.Value },
        };
        var group = await _services.TableClient.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return ValidationProblem("couldn't find group");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, list, new GroupScopedOperationAuthorizationRequirement{
            Group = group,
            Requirement = CrudRequirements.Create,
        });
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        await _services.TableClient.AddEntityAsync(group.Entity);
        return new JsonResult(group);
    }

    // [HttpGet]
    // public async Task<IActionResult> List()
    // {
    //     var items = _services.TableClient.QueryAsync<GroupEntity>(filter: $"PartitionKey eq 'group'").WhereAwait(async groupEntity =>
    //     {
    //         var group = new Group { Entity = groupEntity };
    //         var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
    //         return authResult.Succeeded;
    //     });
    //     return new JsonResult(await items.Select(x => new Group{Entity=x}).ToArrayAsync());
    // }


    // [HttpGet("{id}")]
    // public async Task<IActionResult> GetById(Guid id)
    // {
    //     var group = await _services.TableClient.GetGroupByIdAsync(id);
    //     if (group == null)
    //     {
    //         return NotFound();
    //     }

    //     var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
    //     if (!authResult.Succeeded)
    //     {
    //         return new ForbidResult();
    //     }

    //     return new JsonResult(new {
    //         group,
    //         cards = new List<string>{ "card1", "card2"},
    //     });
    // }

    // public class JoinGroupPayload
    // {
    //     public Guid GroupId { get; set; }
    //     public string GroupPassword { get; set; }
    // }

    // [HttpPost("join")]
    // public async Task<IActionResult> JoinGroup([FromBody] JoinGroupPayload payload)
    // {
    //     // lookup group
    //     var group = await _services.TableClient.GetGroupByIdAsync(payload.GroupId);
    //     if (group == null)
    //     {
    //         return NotFound();
    //     }

    //     // validate join request
    //     var auth = await _services.AuthService.AuthorizeAsync(
    //         HttpContext.User,
    //         group,
    //         new JoinGroupAuthorizationRequirement
    //         {
    //             UserProvidedPassword = payload.GroupPassword
    //         }
    //     );

    //     if (!auth.Succeeded)
    //     {
    //         return Problem(
    //             detail: string.Join(",", auth.Failure?.FailureReasons.Select(x => x.Message) ?? new string[0]),
    //             statusCode: StatusCodes.Status403Forbidden
    //         );
    //     }


    //     // add user to group
    //     group.Members = group.Members.Append(HttpContext.User.GetUserId()!.Value).ToArray();
    //     await _services.TableClient.UpdateEntityAsync(group.Entity, group.Entity.ETag);
    //     return Ok();
    // }

    // public class DeleteGroupPayload
    // {
    //     public Guid GroupId { get; set; }
    // }
    // [HttpDelete]
    // public async Task<IActionResult> Delete([FromBody] DeleteGroupPayload payload)
    // {
    //     var group = await _services.TableClient.GetGroupByIdAsync(payload.GroupId);
    //     if (group == null)
    //     {
    //         return ValidationProblem("couldn't find group");
    //     }

    //     var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Delete);
    //     if (!authResult.Succeeded)
    //     {
    //         return new ForbidResult();
    //     }

    //     await _services.TableClient.DeleteEntityAsync(group.Entity.PartitionKey, group.Entity.RowKey);
    //     return Ok();
    // }
}