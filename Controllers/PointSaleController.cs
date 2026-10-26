using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.PointSale;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT obligatorio en todo el controller
public class PointSaleController : ControllerBase
{
    private readonly PointSaleService _service;

    public PointSaleController(PointSaleService service)
    {
        _service = service;
    }

    // listar todos los puntos de venta
    [HttpGet]
    [Authorize(Roles = "ADMIN")] // Roles (plural), no Role, es una convencion de ASP.NET Core
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    // listar informacion del pos mediante su ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound(new { message = "Punto de venta no encontrado" });
        }

        return Ok(item);
    }

    // crear un nuevo punto de venta
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreatePointSaleRequest request)
    {
        var (result, error) = await _service.CreateAsync(request);

        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result!.IdPointSale }, result);
    }

    // actualizar un punto de venta
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePointSaleRequest request)
    {
        var (result, error)  = await _service.UpdateAsync(id, request);

        if (error is not null)
        {
            if (error.Contains("no encontrado", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = error });
            }

            return BadRequest(new { message = error });
        }

        return Ok(result);
    } 
}
