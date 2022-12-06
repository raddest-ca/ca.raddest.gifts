using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GiftsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace GiftsApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class TokenController : ControllerBase
{
    private readonly Services<TokenController> _services;

    public TokenController(Services<TokenController> services)
    {
        _services = services;
    }

    public class Payload
    {
        public string UserLoginName { get; set; }
        public string UserPassword { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Payload request)
    {
        var user = await _services.TableClient.GetUserByUsernameAsync(request.UserLoginName);
        if (user == null)
        {
            return NotFound();
        }
        if (!BC.Verify(request.UserPassword, user.Password))
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
        return new JsonResult(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
