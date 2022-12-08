using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/group/{groupId:guid}/[controller]")]
public class WishlistController : ControllerBase
{
    private readonly Services<WishlistController> _services;

    public WishlistController(Services<WishlistController> services)
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
            GroupId = groupId,
            DisplayName = payload.DisplayName,
            Owners = new Guid[] { HttpContext.User.GetUserId()!.Value },
        };
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return ValidationProblem("couldn't find group");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, list, new GroupScopedOperationAuthorizationRequirement
        {
            Group = group,
            Requirement = CrudRequirements.Create,
        });
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        await _services.TableClient.AddEntityAsync(list.Entity);
        return new JsonResult(list);
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid groupId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null) {
            return NotFound("Group not found");
        }
        var requirement = new GroupScopedOperationAuthorizationRequirement{
            Group = group,
            Requirement = CrudRequirements.Read,
        };
        var items = _services.TableClient
            .QueryAsync<WishlistEntity>(filter: $"PartitionKey eq 'Group:{groupId}:Wishlist'")
            .Select(entity => new Wishlist { Entity = entity })
            .WhereAwait(async resource =>
            {
                var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, resource, requirement);
                return authResult.Succeeded;
            });
        return new JsonResult(await items.ToArrayAsync());
    }
}