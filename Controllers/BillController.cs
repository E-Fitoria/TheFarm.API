using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheFarm.API.DTOs.Bill;
using TheFarm.API.Services;

namespace TheFarm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillController : ControllerBase
{
    private readonly BillService _service;

    public BillController(BillService service)
    {
        _service = service;
    }

    /// <summary>Listar facturas. Filtros: ?idSale=1&amp;idClient=2</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? idSale,
        [FromQuery] int? idClient)
    {
        var items = await _service.GetAllAsync(idSale, idClient);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null
            ? NotFound(new { message = "Factura no encontrada." })
            : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBillRequest request)
    {
        var (result, error) = await _service.CreateAsync(request);
        if (error is not null)
            return BadRequest(new { message = error });

        return CreatedAtAction(nameof(GetById), new { id = result!.IdBill }, result);
    }

    [HttpPost("{id:int}/mark-sent")]
    public async Task<IActionResult> MarkSent(int id)
    {
        var (result, error) = await _service.MarkSentAsync(id);
        if (error is not null)
        {
            if (error.Contains("no encontrada", StringComparison.OrdinalIgnoreCase))
                return NotFound(new { message = error });
            return BadRequest(new { message = error });
        }
        return Ok(result);
    }
}
