using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/group/{groupId:guid}/wishlist/{wishlistId:guid}/[controller]")]
public class CardController : ControllerBase
{
    private readonly Services<CardController> _services;

    public CardController(Services<CardController> services)
    {
        _services = services;
    }


    public class CreateCardPayload
    {
        public string Content { get; set; }
        public bool VisibileToListOwner {get; set;}
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid groupId, Guid wishlistId, [FromBody] CreateCardPayload payload)
    {

        var resource = new Card
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            WishlistId = wishlistId,
            Content = payload.Content,
            VisibleToListOwner = payload.VisibileToListOwner,
        };
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return ValidationProblem("couldn't find group");
        }

        var wishlist = await _services.TableClient.GetWishlistIfExistsAsync(groupId, wishlistId);
        if (wishlist == null)
        {
            return ValidationProblem("couldn't find wishlist");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, resource, new WishlistScopedOperationAuthorizationRequirement
        {
            Wishlist = wishlist,
            Group = group,
            Requirement = CrudRequirements.Create,
        });
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        await _services.TableClient.AddEntityAsync(resource.Entity);
        return new JsonResult(resource);
    }
}