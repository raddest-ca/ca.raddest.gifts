using System.Globalization;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GiftsApi.Models;

public class User : ITableEntity
{

    public string PartitionKey {
        get => "user";
        set {
            if (value != "user")
                throw new System.NotImplementedException(value);
        }
    }

    public string RowKey { get; set; }

    public Guid Id
    {
        get => Guid.Parse(RowKey);
        set => RowKey = value.ToString();
    }

    public string Username { get; set; }
    public string DisplayName { get; set; }
    public string Password { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}