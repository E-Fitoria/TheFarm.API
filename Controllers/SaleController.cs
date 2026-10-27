using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.Sale;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleController : ControllerBase
{
    private readonly SaleService _service;

    public SaleController(SaleService service)
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

    /// <summary>Listar ventas. Filtros: ?idPointSale=1&amp;idUser=2</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? idPointSale,
        [FromQuery] int? idUser)
    {
        var items = await _service.GetAllAsync(idPointSale, idUser);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { message = "Venta no encontrada." })
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request)
    {
        var userId = GetCurrentUserId();
        var (result, error) = await _service.CreateAsync(request, userId);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdSale }, result);
    }
}
