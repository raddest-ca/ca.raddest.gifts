using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GiftsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
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


    private string CreateJwt(User user)
    {

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

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken CreateRefreshToken(User user)
    {
        var randomNumber = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(randomNumber);
        return new RefreshToken
        {
            UserId = user.Id,
            // sanitize characters for table storage row key
            // https://stackoverflow.com/a/47049320/11141271
            Token = Convert.ToBase64String(randomNumber).Replace("/","_").Replace("+","-"),
        };
    }


    public class LoginPayload
    {
        public string UserLoginName { get; set; }
        public string UserPassword { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] LoginPayload request)
    {
        var user = await _services.TableClient.GetUserByUsernameAsync(request.UserLoginName);
        if (user == null)
        {
            return ValidationProblem("Couldn't find user");
        }
        if (!BC.Verify(request.UserPassword, user.Password))
        {
            return Unauthorized();
        }

        var refreshToken = CreateRefreshToken(user);
        await _services.TableClient.AddEntityAsync(refreshToken.Entity);

        return new JsonResult(new
        {
            token = CreateJwt(user),
            refreshToken = $"{refreshToken.UserId}:{refreshToken.Token}",
        });
    }



    public class LoginRefreshPayload
    {
        public string RefreshToken {get; set;}
    }
    [HttpPost]
    [Route("refresh")]
    public async Task<IActionResult> Refresh([FromBody] LoginRefreshPayload request)
    {
        Console.WriteLine("got " + request.RefreshToken);
        if (!request.RefreshToken.Contains(":"))
        {
            return BadRequest("bad refresh token");
        }
        var parts = request.RefreshToken.Split(":");
        if (parts.Length != 2)
        {
            return BadRequest("bad refresh token");
        }
        if (!Guid.TryParse(parts[0], out var userId))
        {
            return BadRequest("bad refresh token");
        }
        var token = new RefreshToken
        {
            UserId = userId,
            Token = parts[1],
        };

        var userEntity = await _services.TableClient.GetEntityAsync<UserEntity>("User", userId.ToString());
        if (userEntity == null)
        {
            return BadRequest("user does not exist");
        }

        if (!(await _services.TableClient.GetEntityIfExistsAsync<RefreshTokenEntity>(token.Entity.PartitionKey, token.Entity.RowKey)).HasValue)
        {
            return Unauthorized();
        }

        return new JsonResult(new
        {
            token = CreateJwt(new User { Entity = userEntity }),
        });
    }
}
