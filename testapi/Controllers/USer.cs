using Microsoft.AspNetCore.Mvc;

namespace testapi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    public String Get ()
    {
        return "Hello";
    }
    [HttpGet("{id}")]
    public String Get (int id)
    {
        return "Hello" + id;
    }
}
