using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.Auth;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController] // indica que es un controlador de API
[Route("api/[controller]")] // indica la ruta base para las rutas de este controlador
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [AllowAnonymous] // indica que esta ruta es accesible sin autenticacion
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize] // indica que esta ruta es accesible solo con autenticacion
    public IActionResult Me()
    {
        return Ok(new
        {
            idUser = User.FindFirst("idUser")?.Value,
            username = User.Identity?.Name,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }
}