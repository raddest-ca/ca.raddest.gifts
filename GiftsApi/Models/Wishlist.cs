using System.Text.Json.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GiftsApi.Models;

public class Wishlist
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public string DisplayName { get; set; }
    public Guid[] Owners { get; set; }


    [JsonIgnore]
    private ETag _eTag { get; set; }

    [JsonIgnore]
    public WishlistEntity Entity
    {
        get => new WishlistEntity
        {
            PartitionKey = $"Group:{GroupId}:Wishlist",
            RowKey = Id.ToString(),
            DisplayName = DisplayName,
            Owners = string.Join(",", Owners),
            ETag = _eTag,
        };
        set => (Id, GroupId, DisplayName, Owners, _eTag) = (Guid.Parse(value.RowKey), Guid.Parse(value.PartitionKey.Split(":")[1]), value.DisplayName, value.Owners.Split(',').Select(Guid.Parse).ToArray(), value.ETag);
    }
}

public class WishlistEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string DisplayName { get; set; }
    public string Owners { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}