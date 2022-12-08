using GiftsApi.Auth;
using GiftsApi.Services;
using Microsoft.AspNetCore.Authorization;
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
        public string UserLoginName { get; set; }
        public string UserPassword { get; set; }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserPayload payload)
    {
        var user = await _services.TableClient.GetUserByUsernameIfExistsAsync(payload.UserLoginName);
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

    public class DisplayNameUpdatePayload
    {
        public string UserId {get; set;}
        public string DisplayName { get; set; }
    }

    [HttpPost]
    [Route("displayName")]
    [Authorize]
    public async Task<IActionResult> SetDisplayName([FromBody] DisplayNameUpdatePayload payload)
    {
        if (!Guid.TryParse(payload.UserId, out var userId))
        {
            return BadRequest("invalid user id");
        }

        var user = await _services.TableClient.GetUserIfExistsAsync(userId);
        if (user == null)
        {
            return BadRequest("user not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, user, CrudRequirements.Update);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        user.DisplayName = payload.DisplayName;
        _services.TableClient.UpdateEntity(user.Entity, user.Entity.ETag);
        return new JsonResult(user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var resource = await _services.TableClient.GetUserIfExistsAsync(id);
        if (resource == null)
        {
            return BadRequest("user not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, resource, CrudRequirements.Read);
        if (!authResult.Succeeded)
        {
            return new ForbidResult();
        }

        return new JsonResult(resource);
    }
}
