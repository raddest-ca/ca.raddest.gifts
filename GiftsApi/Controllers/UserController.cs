using GiftsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GiftsApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly Services<TokenController> _services;

    public UserController(Services<TokenController> services)
    {
        _services = services;
    }

    public class CreateUserPayload
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserPayload payload)
    {
        var user = await _services.TableClient.GetUserByUsernameAsync(payload.Username);
        if (user != null) {
            return Conflict();
        }
        user = new User
        {
            Username = payload.Username,
            Password = BC.HashPassword(payload.Password),
            DisplayName = payload.Username,
            Id = Guid.NewGuid(),
        };
        _services.TableClient.AddEntity(user);
        return new JsonResult(user);
    }
}
