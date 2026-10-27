using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.InventoryTransfer;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryTransferController : ControllerBase
{
    private readonly InventoryTransferService _service;

    public InventoryTransferController(InventoryTransferService service)
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
        => Ok(await _service.GetAllAsync(status));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { message = "Transferencia no encontrada." })
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInventoryTransferRequest request)
    {
        var userId = GetCurrentUserId();
        var (result, error) = await _service.CreateAsync(request, userId);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdInventoryTransfer }, result);
    }

    [HttpPost("{id:int}/decide")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Decide(int id, [FromBody] DecideTransferRequest request)
    {
        var userId = GetCurrentUserId();
        var (result, error) = await _service.DecideAsync(id, request, userId);
        if (error is not null)
        {
            if (error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });
            return BadRequest(new { message = error });
        }
        return Ok(result);
    }
}