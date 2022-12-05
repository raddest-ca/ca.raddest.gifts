
using Azure.Data.Tables;
using GiftsApi.Models;

namespace GiftsApi.Extensions;

public static class TableServiceExtensions
{
    public static async Task<Group?> GetGroupByIdAsync(this TableClient table, Guid id)
    {
        return await table.QueryAsync<Group>(filter: $"PartitionKey eq 'group' and RowKey eq '{id}'").FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUserByIdAsync(this TableClient table, Guid id)
    {
        return await table.QueryAsync<User>(filter: $"PartitionKey eq 'user' and RowKey eq '{id}'").FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUserByUsernameAsync(this TableClient table, string username)
    {
        return await table.QueryAsync<User>(filter: $"PartitionKey eq 'user' and Username eq '{username}'").FirstOrDefaultAsync();
    }
}