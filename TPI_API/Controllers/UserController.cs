using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TPI_API.Models;

namespace TPI_API.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public UserController(UserManager<User> userManager)
    {
        _userManager = userManager;

    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = $"Usuario con ID {id} no encontrado." });
        }
        return Ok(user);
    }
}
