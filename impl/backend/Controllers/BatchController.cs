using Microsoft.AspNetCore.Mvc;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
using GiftsApi.Auth;

namespace GiftsApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BatchController : ControllerBase
{
    private readonly Services<BatchController> _services;

    public BatchController(Services<BatchController> services)
    {
        _services = services;
    }

    [Route("group/{groupId:guid}")]
    [HttpGet]
    public async Task<IActionResult> GetGroupDetails(Guid groupId)
    {
        var group = await _services.TableClient.GetGroupIfExistsAsync(groupId);
        if (group == null)
        {
            return BadRequest("group not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, group, CrudRequirements.Read);
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        var wishlistReadRequirement = new GroupScopedOperationAuthorizationRequirement {
            Group = group,
            Requirement = CrudRequirements.Read,
        };

        var wishlists = await _services.TableClient.QueryAsync<WishlistEntity>(filter: $"PartitionKey eq 'Group:{groupId}:Wishlist'")
            .Select(x => new Wishlist { Entity = x })
            .WhereAwait(async x => (await _services.AuthService.AuthorizeAsync(HttpContext.User, x, wishlistReadRequirement)).Succeeded)
            .ToDictionaryAsync(x => x.Id);

        var cards = new Dictionary<Guid,Dictionary<Guid,Card>>();
        foreach (var wishlist in wishlists.Values)
        {
            var cardReadRequirement = new WishlistScopedOperationAuthorizationRequirement {
                Group = group,
                Wishlist = wishlist,
                Requirement = CrudRequirements.Read,
            };
            cards.Add(
                wishlist.Id,
                await _services.TableClient.QueryAsync<CardEntity>(filter: $"PartitionKey eq 'Group:{groupId}:Wishlist:{wishlist.Id}:Card'")
                    .Select(x => new Card { Entity = x })
                    .WhereAwait(async x => (await _services.AuthService.AuthorizeAsync(HttpContext.User, x, cardReadRequirement)).Succeeded)
                    .ToDictionaryAsync(x => x.Id)
            );
        }
        foreach (var wishlistEntry in cards)
        {
            var wishlist = wishlists[wishlistEntry.Key];
            if (wishlist.Owners.Contains(User.GetUserId()!.Value))
            {
                foreach (var card in wishlistEntry.Value.Values)
                {
                    card.Tags = card.Tags.Where((entry) => entry.Value).ToDictionary(entry => entry.Key, entry => entry.Value);
                }
            }
        }

        
        var users = await group.Members
            .Union(group.Owners)
            .Union(wishlists.Values.SelectMany(x => x.Owners))
            .ToAsyncEnumerable()
            .SelectAwait(async x => await _services.TableClient.GetEntityIfExistsAsync<UserEntity>("User", x.ToString()))
            .Where(x => x.HasValue)
            .Select(x => new User { Entity = x.Value })
            .WhereAwait(async x => (await _services.AuthService.AuthorizeAsync(HttpContext.User, x, CrudRequirements.Read)).Succeeded)
            .ToDictionaryAsync(x => x.Id);

        return new JsonResult(new
        {
            group,
            users,
            wishlists,
            cards,
        });
    }
}