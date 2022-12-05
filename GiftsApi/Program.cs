using Azure.Data.Tables;
using Azure.Identity;
using GiftsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential()
);

var config = new AppConfig();
builder.Configuration.Bind(config);
builder.Services.AddSingleton<AppConfig>(config);
builder.Services.AddScoped<TableClient>((services) => {
    var config = services.GetService<AppConfig>()!;
    var serviceClient = new TableClient(config.StorageConnectionString, config.StorageTableName);
    return serviceClient;
});
builder.Services.AddScoped(typeof(Services<>));
builder.Services.AddScoped<GiftServices>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
