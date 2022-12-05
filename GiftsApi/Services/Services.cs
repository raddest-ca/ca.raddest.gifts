using Azure.Data.Tables;
using Microsoft.AspNetCore.Authorization;

namespace GiftsApi.Services;

public class Services<T>
{
    public Services(
        ILogger<T> logger,
        AppConfig config,
        TableClient tableClient,
        IAuthorizationService authService
    )
    {
        Logger = logger;
        Config = config;
        TableClient = tableClient;
        AuthService = authService;
    }

    public ILogger<T> Logger { get; }
    public AppConfig Config { get; }
    public TableClient TableClient { get; }
    public IAuthorizationService AuthService { get; }
}