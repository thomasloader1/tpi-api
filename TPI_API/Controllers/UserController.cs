using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPI_API.Models;

namespace TPI_API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        /*var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound(new { message = $"Usuario con ID {id} no encontrado." });
        }*/
        var usuarioId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(usuarioId);
        var nombreUsuario = User.Identity?.Name;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new
        {
            UsuarioId = usuarioId,
            NombreUsuario = nombreUsuario,
            Email = email,
            User = user,
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.Identity.Name;
        var user = await _userManager.FindByEmailAsync(email);

        return Ok(user);
    }
}
