
using Azure.Data.Tables;
using GiftsApi.Models;

namespace GiftsApi.Extensions;

public static class TableServiceExtensions
{
    public static async Task<Group?> GetGroupByIdAsync(this TableClient table, Guid id)
    {
        var entity = await table.QueryAsync<GroupEntity>(filter: $"PartitionKey eq 'group' and RowKey eq '{id}'").FirstOrDefaultAsync();
        if (entity == null) return null;
        return new Group { Entity = entity };
    }

    public static async Task<User?> GetUserByIdAsync(this TableClient table, Guid id)
    {
        var entity = await table.QueryAsync<UserEntity>(filter: $"PartitionKey eq 'user' and RowKey eq '{id}'").FirstOrDefaultAsync();
        if (entity == null) return null;
        return new User { Entity = entity };
    }

    public static async Task<User?> GetUserByUsernameAsync(this TableClient table, string username)
    {
        var entity = await table.QueryAsync<UserEntity>(filter: $"PartitionKey eq 'user' and Username eq '{username}'").FirstOrDefaultAsync();
        if (entity == null) return null;
        return new User { Entity = entity };   
    }
}