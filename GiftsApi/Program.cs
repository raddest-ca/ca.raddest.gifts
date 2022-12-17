using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using GiftsApi.Auth;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add key vault config
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential()
);
// config POCO
var config = new AppConfig();
builder.Configuration.Bind(config);
builder.Services.AddSingleton<AppConfig>(config);

// data persistence
builder.Services.AddScoped<TableClient>((services) => {
    var config = services.GetService<AppConfig>()!;
    var serviceClient = new TableClient(config.StorageConnectionString, config.StorageTableName);
    return serviceClient;
});

// service helpers
builder.Services.AddScoped(typeof(Services<>));

// auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config.JwtIssuer,
            ValidAudience = config.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.JwtKey))
        };
    });
builder.Services.AddSingleton<IAuthorizationHandler, GroupAuthorizationCrudHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, JoinGroupAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, WishlistAuthorizationCrudHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CardAuthorizationCrudHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, UserAuthorizationCrudHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ModifyGroupUserAuthorizationHandler>();


// cors
// var myCors = "_myAllowSpecificOrigins"
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("configuring cors for dev");
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });
}

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

app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
