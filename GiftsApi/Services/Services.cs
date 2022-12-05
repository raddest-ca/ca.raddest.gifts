namespace GiftsApi.Services;

public class Services<T>
{
    public Services(
        ILogger<T> logger,
        AppConfig config,
        GiftServices giftServices
    ) {
        Logger = logger;
        Config = config;
        GiftServices = giftServices;
    }

    public readonly ILogger<T> Logger;
    public readonly AppConfig Config;

    public readonly GiftServices GiftServices;
}