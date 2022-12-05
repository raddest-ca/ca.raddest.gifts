using Microsoft.AspNetCore.Mvc;
using GiftsApi.Models;
using GiftsApi.Services;
using static GiftsApi.Services.GiftServices;

namespace GiftsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly Services<GroupsController> _services;

    public GroupsController(Services<GroupsController> services)
    {
        _services = services;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] GroupCreatePayload payload)
    {
        return new JsonResult(await _services.GiftServices.CreateGroup(payload));
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return new JsonResult(await _services.GiftServices.GetGroups().ToArrayAsync());
    }
}