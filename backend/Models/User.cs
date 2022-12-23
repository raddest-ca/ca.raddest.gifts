using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GiftsApi.Models;

public class User
{
    public Guid Id { get; set; }

    [JsonIgnore]
    public string LoginName { get; set; }
    public string DisplayName { get; set; }
    [JsonIgnore]
    public string Password { get; set; }

    [JsonIgnore]
    private ETag _eTag { get; set;}

    [JsonIgnore]
    public UserEntity Entity
    {
        get => new UserEntity
        {
            PartitionKey = "User",
            RowKey = Id.ToString(),
            Username = LoginName,
            DisplayName = DisplayName,
            Password = Password,
            ETag = _eTag,
        };
        set => (Id, LoginName, DisplayName, Password, _eTag) = (Guid.Parse(value.RowKey), value.Username, value.DisplayName, value.Password, value.ETag);
    }
}

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}

public class RefreshToken
{
    public Guid UserId {get; set;}
    public string Token { get; set; }
    [JsonIgnore]
    private ETag _eTag { get; set;}
    [JsonIgnore]
    public RefreshTokenEntity Entity
    {
        get => new RefreshTokenEntity
        {
            PartitionKey = $"User:{UserId}:RefreshToken",
            RowKey = Token,
            ETag = _eTag,
        };
        set => (UserId, Token, _eTag) = (Guid.Parse(value.PartitionKey.Split(':')[1]), value.RowKey, value.ETag);
    }
}

public class RefreshTokenEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}