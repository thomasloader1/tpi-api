using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TPI_API.Dtos.User;
using TPI_API.Interfaces;
using TPI_API.Models;

namespace TPI_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;

    }

  

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {   
        var email = User.Identity.Name;
        var user = await _userService.GetUserWithRoles(email);
        if (user == null)
        {
            return Unauthorized(new { message = "Usuario no encontrado." });
        }

        
        return Ok(user);
    }
}
