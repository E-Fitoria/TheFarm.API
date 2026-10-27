using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.Resupply;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResupplyController : ControllerBase
{
    private readonly ResupplyService _service;

    public ResupplyController(ResupplyService service)
    {
        _service = service;
    }

    private int GetCurrentUserId()
    {
        var raw = User.FindFirstValue("idUser")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) ? id : 0;
    }

    /// <summary>Listar reposiciones. Filtros: ?idPointSale=1&amp;status=Pending</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? idPointSale,
        [FromQuery] string? status)
    {
        var items = await _service.GetAllAsync(idPointSale, status);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { message = "Reposición no encontrada." })
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResupplyRequest request)
    {
        var userId = GetCurrentUserId();
        var (result, error) = await _service.CreateAsync(request, userId);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdResupply }, result);
    }

    [HttpPost("{id:int}/complete")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Complete(int id)
    {
        var (result, error) = await _service.CompleteAsync(id);
        if (error is not null)
        {
            if (error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });
            return BadRequest(new { message = error });
        }
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Cancel(int id)
    {
        var (result, error) = await _service.CancelAsync(id);
        if (error is not null)
        {
            if (error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });
            return BadRequest(new { message = error });
        }
        return Ok(result);
    }
}
