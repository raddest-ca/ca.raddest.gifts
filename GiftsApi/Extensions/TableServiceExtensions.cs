
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

    public static async Task<Group?> GetGroupIfExistsAsync(this TableClient table, Guid id)
    {
        var entity = await table.GetEntityIfExistsAsync<GroupEntity>("Group", id.ToString());
        if (!entity.HasValue) return null;
        return new Group { Entity = entity.Value };
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
}