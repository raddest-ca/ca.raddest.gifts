namespace Gifts.Services;

public class AppConfig
{
    public string KeyVaultName { get; set; }
    public string StorageConnectionString { get; set; }
    public string StorageTableName { get; set; }

    public string JwtKey { get; set; }
    public string JwtIssuer { get; set; }
    public string JwtAudience { get; set; }
}