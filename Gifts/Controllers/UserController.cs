using Microsoft.AspNetCore.Mvc;

namespace Gifts.Controllers;
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
        public string UserLoginName { get; set; }
        public string UserPassword { get; set; }
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserPayload payload)
    {
        var user = await _services.TableClient.GetUserByUsernameAsync(payload.UserLoginName);
        if (user != null) {
            return Conflict();
        }
        user = new User
        {
            LoginName = payload.UserLoginName,
            Password = BC.HashPassword(payload.UserPassword),
            DisplayName = payload.UserLoginName,
            Id = Guid.NewGuid(),
        };
        _services.TableClient.AddEntity(user.Entity);
        return new JsonResult(user);
    }
}
