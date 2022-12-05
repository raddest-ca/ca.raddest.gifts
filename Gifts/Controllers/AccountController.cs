using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gifts.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Gifts.Controllers;

public class AccountController : Controller
{
    private readonly Services<AccountController> _services;

    public class LoginViewModel
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public AccountController(Services<AccountController> services)
    {
        _services = services;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Login()
    {
        return View();
    }

    public async Task<IActionResult> LoginSubmit(LoginViewModel model)
    {
        _services.Logger.LogInformation($"Logging in {model.Username}");
        var user = await _services.TableClient.GetUserByUsernameAsync(model.Username);
        if (user == null)
        {
            return NotFound();
        }
        if (!BC.Verify(model.Password, user.Password))
        {
            return Unauthorized();
        }
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.LoginName),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_services.Config.JwtKey));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _services.Config.JwtIssuer,
            audience: _services.Config.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: signIn
        );
        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);
        HttpContext.Session.SetString("Token", serializedToken);
        return RedirectToAction("Index", "Home");
    }
}
