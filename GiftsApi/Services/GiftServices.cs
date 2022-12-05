using Azure.Data.Tables;
using GiftsApi.Models;
using BC = BCrypt.Net.BCrypt;

namespace GiftsApi.Services;

public class GiftServices
{
    private readonly ILogger<GiftServices> _logger;
    private readonly AppConfig _config;
    private readonly TableClient _table;

    public GiftServices(
        ILogger<GiftServices> logger,
        AppConfig config,
        TableClient tableClient
    ) {
        _logger = logger;
        _config = config;
        _table = tableClient;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        return await _table.QueryAsync<User>(filter: $"PartitionKey eq 'user' and Username eq '{username}'").FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserById(Guid id)
    {
        return await _table.QueryAsync<User>(filter: $"PartitionKey eq 'user' and RowKey eq '{id}'").FirstOrDefaultAsync();
    }

    public class CreateUserPayload
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public async Task CreateUser(CreateUserPayload payload)
    {
        var user = await GetUserByUsername(payload.Username);
        if (user != null) {
            throw new ArgumentException("User already exists");
        }
        user = new User
        {
            Username = payload.Username,
            Password = BC.HashPassword(payload.Password),
            DisplayName = payload.Username,
            Id = Guid.NewGuid(),
        };
        _table.AddEntity(user);
    }

    public class GroupCreatePayload
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public async Task<Group> CreateGroup(GroupCreatePayload payload) 
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            DisplayName = payload.Name,
            Password = BC.HashPassword(payload.Password),
        };
        await _table.AddEntityAsync(group);
        return group;
    }

    public IAsyncEnumerable<Group> GetGroups()
    {
        return _table.QueryAsync<Group>(filter: $"PartitionKey eq 'group'");
    }
}