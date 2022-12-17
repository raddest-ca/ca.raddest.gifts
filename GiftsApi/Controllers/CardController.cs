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
        public bool VisibleToListOwners {get; set;}
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid groupId, Guid wishlistId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("group not found");
        }

        var wishlist = await _services.TableClient.GetWishlistIfExistsAsync(groupId, wishlistId);
        if (wishlist == null)
        {
            return BadRequest("wishlist not found");
        }

        var requirement = new WishlistScopedOperationAuthorizationRequirement
        {
            Wishlist = wishlist,
            Group = group,
            Requirement = CrudRequirements.Read,
        };

        var cards = await _services.TableClient.QueryAsync<CardEntity>(filter: $"PartitionKey eq 'Group:{groupId}:Wishlist:{wishlistId}:Card'")
            .Select(cardEntity => new Card { Entity = cardEntity})
            .WhereAwait(async card => (await _services.AuthService.AuthorizeAsync(HttpContext.User, card, requirement)).Succeeded)
            .ToArrayAsync();

        return new JsonResult(cards);
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
            VisibleToListOwners = payload.VisibleToListOwners,
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

    public class CardUpdatePayload
    {
        public string? Content { get; set; }
        public bool? VisibleToListOwners { get; set; }
    }

    [HttpPatch("{cardId:guid}")]
    public async Task<IActionResult> Update(Guid groupId, Guid wishlistId, Guid cardId, [FromBody] CardUpdatePayload payload)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("couldn't find group");
        }

        var wishlist = await _services.TableClient.GetWishlistIfExistsAsync(groupId, wishlistId);
        if (wishlist == null)
        {
            return BadRequest("couldn't find wishlist");
        }

        var card = await _services.TableClient.GetCardIfExistsAsync(groupId, wishlistId, cardId);
        if (card == null)
        {
            return BadRequest("couldn't find card");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, card, new WishlistScopedOperationAuthorizationRequirement
        {
            Wishlist = wishlist,
            Group = group,
            Requirement = CrudRequirements.Update,
        });
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        if (payload.Content != null)
        {
            card.Content = payload.Content;
        }

        if (payload.VisibleToListOwners != null)
        {
            card.VisibleToListOwners = payload.VisibleToListOwners.Value;
        }

        await _services.TableClient.UpdateEntityAsync(card.Entity, card.Entity.ETag);
        return new JsonResult(card);
    }

    [HttpDelete("{cardId:guid}")]
    public async Task<IActionResult> Delete(Guid groupId, Guid wishlistId, Guid cardId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("couldn't find group");
        }

        var wishlist = await _services.TableClient.GetWishlistIfExistsAsync(groupId, wishlistId);
        if (wishlist == null)
        {
            return BadRequest("couldn't find wishlist");
        }

        var card = await _services.TableClient.GetCardIfExistsAsync(groupId, wishlistId, cardId);
        if (card == null)
        {
            return BadRequest("couldn't find card");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, card, new WishlistScopedOperationAuthorizationRequirement
        {
            Wishlist = wishlist,
            Group = group,
            Requirement = CrudRequirements.Delete,
        });
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        await _services.TableClient.DeleteCardAsync(card);
        return Ok();
    }
}