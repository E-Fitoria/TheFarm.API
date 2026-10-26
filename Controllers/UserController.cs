using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.User;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);

        if (item is null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var (result, error) = await _service.CreateAsync(request);

        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdUser }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var (result, error) = await _service.UpdateAsync(id, request);

        if (error is not null)
        {
            if (error.Contains("no existe", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(result);
    }
}
