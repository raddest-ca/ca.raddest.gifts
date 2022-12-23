
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Data.Tables;
using GiftsApi.Models;

namespace GiftsApi.Extensions;

public static class TableServiceExtensions
{
    public static async Task<Wishlist?> GetWishlistIfExistsAsync(this TableClient table, Guid groupId, Guid wishlistId)
    {
        var entity = await table.GetEntityIfExistsAsync<WishlistEntity>($"Group:{groupId}:Wishlist",wishlistId.ToString());
        if (!entity.HasValue) return null;
        return new Wishlist { Entity = entity.Value };
    }

    public static async Task<Card?> GetCardIfExistsAsync(this TableClient table, Guid groupId, Guid wishlistId, Guid cardId)
    {
        var entity = await table.GetEntityIfExistsAsync<CardEntity>($"Group:{groupId}:Wishlist:{wishlistId}:Card", cardId.ToString());
        if (!entity.HasValue) return null;
        return new Card { Entity = entity.Value };
    }

    public static async Task<Group?> GetGroupIfExistsAsync(this TableClient table, Guid id)
    {
        var entity = await table.GetEntityIfExistsAsync<GroupEntity>("Group", id.ToString());
        if (!entity.HasValue) return null;
        return new Group { Entity = entity.Value };
    }

    public static async Task DeleteGroupAsync(this TableClient table, Group group)
    {
        var wishlists = await table.QueryAsync<WishlistEntity>(filter: $"PartitionKey eq 'Group:{group.Id}:Wishlist'")
            .Select(x => new Wishlist{Entity=x})
            .ToListAsync();
        foreach (var wishlist in wishlists)
        {
            await table.DeleteWishlistAsync(wishlist);
        }
        await table.DeleteEntityAsync(group.Entity.PartitionKey, group.Entity.RowKey, group.Entity.ETag);
    }

    public static async Task DeleteWishlistAsync(this TableClient table, Wishlist wishlist)
    {
        var cards = await table.QueryAsync<CardEntity>(filter: $"PartitionKey eq 'Group:{wishlist.GroupId}:Wishlist:{wishlist.Id}:Card'")
            .Select(x => new Card{Entity=x})
            .ToListAsync();
        foreach (var card in cards)
        {
            await table.DeleteCardAsync(card);
        }
        await table.DeleteEntityAsync(wishlist.Entity.PartitionKey, wishlist.Entity.RowKey, wishlist.Entity.ETag);
    }

    public static async Task DeleteCardAsync(this TableClient table, Card card)
    {
        await table.DeleteEntityAsync(card.Entity.PartitionKey, card.Entity.RowKey, card.Entity.ETag);
    }

    public static async Task<User?> GetUserIfExistsAsync(this TableClient table, Guid id)
    {
        var entity=  await table.GetEntityIfExistsAsync<UserEntity>("User", id.ToString());
        if (!entity.HasValue) return null;
        return new User { Entity = entity.Value };
    }

    public static async Task<User?> GetUserByUsernameIfExistsAsync(this TableClient table, string username)
    {
        var entity = await table.QueryAsync<UserEntity>(filter: $"PartitionKey eq 'User' and Username eq '{username}'").FirstOrDefaultAsync();
        if (entity == null) return null;
        return new User { Entity = entity };   
    }

    public static async Task<User?> GetUserIfExistsAsync(this TableClient table, ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;
        if (Guid.TryParse(userId, out var guid)) return await table.GetUserIfExistsAsync(guid);
        return null;
    }
}