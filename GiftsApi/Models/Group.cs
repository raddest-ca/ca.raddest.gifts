using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GiftsApi.Models;

public class Group
{

    public string DisplayName { get; set; }
    public string Password { get; set; }

    public Guid[] Members { get; set; }
    public Guid[] Owners { get; set; }

    public Guid Id { get; set; }
    [JsonIgnore]

    private ETag _eTag { get; set;}

    [JsonIgnore]
    public GroupEntity Entity
    {
        get => new GroupEntity
        {
            PartitionKey = "Group",
            RowKey = Id.ToString(),
            DisplayName = DisplayName,
            Password = Password,
            Members = string.Join(",", Members),
            Owners = string.Join(",", Owners),
            ETag = _eTag,
        };
        set => (Id, DisplayName, Password, Members, Owners, _eTag) = (Guid.Parse(value.RowKey), value.DisplayName, value.Password, value.Members.Split(',').Select(Guid.Parse).ToArray(), value.Owners.Split(',').Select(Guid.Parse).ToArray(), value.ETag);
    }
}

public class GroupEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public string Members { get; set; }
    public string Owners { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}