using Microsoft.AspNetCore.Mvc;
using GiftsApi.Models;

namespace GiftsApi.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupsController : ControllerBase
{
    private readonly ILogger<GroupsController> _logger;

    public GroupsController(ILogger<GroupsController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name="GetGroups")]
    public IEnumerable<Group> Get()
    {
        return new List<Group>(){
            new Group {
                Id = Guid.NewGuid(),
                Name = "Test Group",
                Password = "Test Password",
                Members = new Guid[] {Guid.NewGuid(), Guid.NewGuid()}
            }
        };
    }

    [HttpPost(Name="CreateGroup")]
    public Group Create([FromBody] Group group)
    {
        return group;
    }
}