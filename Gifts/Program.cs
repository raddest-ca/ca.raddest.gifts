
using System.Net;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Tailwind;

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
builder.Services.AddScoped<TableClient>((services) =>
{
    var config = services.GetService<AppConfig>()!;
    var serviceClient = new TableClient(config.StorageConnectionString, config.StorageTableName);
    return serviceClient;
});

// service helpers
builder.Services.AddScoped(typeof(Services<>));

// auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
builder.Services.AddSession();

// Add services to the container.
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.RunTailwind("css:build", "./");

    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseSession();
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token))
    {
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }
    await next();
});

app.UseStaticFiles();
app.UseStatusCodePages(async context =>
{
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        response.Redirect("/Account/Login");
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
