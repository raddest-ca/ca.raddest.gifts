using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace Gifts.Models;

public class User
{
    public Guid Id { get; set; }

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
            PartitionKey = "user",
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