using GiftsApi.Models;
using GiftsApi.Services;
using Microsoft.AspNetCore.Mvc;
using static GiftsApi.Services.GiftServices;

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


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserPayload payload)
    {
        try {
            await _services.GiftServices.CreateUser(payload);
        } catch (ArgumentException e) {
            return BadRequest(e.Message);
        }
        return Ok();
    }
}
