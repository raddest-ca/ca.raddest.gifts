using System.Globalization;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;

namespace GiftsApi.Models;

public class Group : ITableEntity
{
    public string PartitionKey {
        get => "group";
        set {
            if (value != "group")
                throw new System.NotImplementedException(value);
        }
    }

    public string RowKey { get; set; }

    public string DisplayName { get; set; }
    public string Password { get; set; }

    [IgnoreDataMember]
    public Guid Id
    {
        get => Guid.Parse(RowKey);
        set => RowKey = value.ToString();
    }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
