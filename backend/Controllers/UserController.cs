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

    public class UpdateUserPayload
    {
        public string? DisplayName { get; set; }
        public string? Password { get; set; }
        public string? LoginName {get; set;}
    }

    [HttpPatch]
    [Route("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserPayload payload)
    {
        if (payload.DisplayName != null && payload.DisplayName.Trim().Length == 0)
        {
            return BadRequest("display name empty");
        }
        if (payload.LoginName != null && payload.LoginName.Trim().Length == 0)
        {
            return BadRequest("display name empty");
        }

        var resource = await _services.TableClient.GetUserIfExistsAsync(id);
        if (resource == null)
        {
            return BadRequest("user not found");
        }

        var authResult = await _services.AuthService.AuthorizeAsync(HttpContext.User, resource, CrudRequirements.Update);
        if (!authResult.Succeeded)
        {
            return authResult.ToActionResult();
        }

        if (payload.DisplayName != null)
        {
            resource.DisplayName = payload.DisplayName;
        }

        if (payload.Password != null)
        {
            resource.Password = BC.HashPassword(payload.Password);
        }

        if (payload.LoginName != null)
        {
            var existingUser = await _services.TableClient.GetUserByUsernameIfExistsAsync(payload.LoginName);
            if (existingUser != null)
            {
                return Conflict("username already exists");
            }
            resource.LoginName = payload.LoginName;
        }

        _services.TableClient.UpdateEntity(resource.Entity, resource.Entity.ETag);
        return new JsonResult(resource);
    }
}
