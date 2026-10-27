using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.Inventory;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _service;

    public InventoryController(InventoryService service)
    {
        _service = service;
    }

    /// <summary>Listar inventario. Filtrar por sucursal: ?idPointSale=1</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? idPointSale,
        [FromQuery] int? idProduct)
    {
        var items = await _service.GetAllAsync(idPointSale, idProduct);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { message = "Inventario no encontrado." })
            : Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateInventoryRequest request)
    {
        var (result, error) = await _service.CreateAsync(request);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdInventory }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateInventoryRequest request)
    {
        var (result, error) = await _service.UpdateAsync(id, request);
        if (error is not null)
        {
            if (error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });
            return BadRequest(new { message = error });
        }
        return Ok(result);
    }
}