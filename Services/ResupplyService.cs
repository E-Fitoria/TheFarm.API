using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Resupply;

namespace TheFarm.API.Services;

public class ResupplyService
{
    private readonly AppDbContext _db;

    public ResupplyService(AppDbContext db)
    {
        _db = db;
    }

    private static ResupplyResponse MapToResponse(Resupply r) => new()
    {
        IdResupply = r.IdResupply,
        Status = r.Status,
        Description = r.Description,
        IdPointSale = r.IdPointSale,
        PointSaleName = r.PointSale?.Name,
        IdUser = r.IdUser,
        Username = r.User?.Username,
        Details = r.ResupplyDetails.Select(d => new ResupplyDetailDto
        {
            IdResupplyDetail = d.IdResupplyDetail,
            IdProduct = d.IdProduct,
            ProductName = d.Product?.Name,
            Quantity = d.Quantity
        }).ToList()
    };

    private IQueryable<Resupply> BaseQuery() =>
        _db.Resupplies
            .Include(r => r.PointSale)
            .Include(r => r.User)
            .Include(r => r.ResupplyDetails)
                .ThenInclude(d => d.Product);

    public async Task<List<ResupplyResponse>> GetAllAsync(int? idPointSale, string? status)
    {
        var query = BaseQuery().AsNoTracking();

        if (idPointSale.HasValue)
            query = query.Where(r => r.IdPointSale == idPointSale.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(r => r.Status == status.Trim());

        var items = await query
            .OrderByDescending(r => r.IdResupply)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ResupplyResponse?> GetByIdAsync(int id)
    {
        var item = await BaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdResupply == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(ResupplyResponse? Result, string? Error)> CreateAsync(
        CreateResupplyRequest request,
        int requestingUserId)
    {
        if (requestingUserId <= 0)
            return (null, "Usuario solicitante inválido.");

        if (request.Details is null || request.Details.Count == 0)
            return (null, "Debe indicar al menos un detalle de reposición.");

        if (request.Details.Any(d => d.Quantity <= 0))
            return (null, "Las cantidades deben ser mayores que cero.");

        var posExists = await _db.PointSales.AnyAsync(p => p.IdPointSale == request.IdPointSale);
        if (!posExists)
            return (null, "El punto de venta no existe.");

        var userExists = await _db.Users.AnyAsync(u => u.IdUser == requestingUserId);
        if (!userExists)
            return (null, "El usuario solicitante no existe.");

        foreach (var detail in request.Details)
        {
            var productExists = await _db.Products.AnyAsync(p => p.IdProduct == detail.IdProduct);
            if (!productExists)
                return (null, $"El producto {detail.IdProduct} no existe.");
        }

        var entity = new Resupply
        {
            Status = "Pending",
            Description = request.Description?.Trim() ?? string.Empty,
            IdPointSale = request.IdPointSale,
            IdUser = requestingUserId
        };

        foreach (var detail in request.Details)
        {
            entity.ResupplyDetails.Add(new ResupplyDetail
            {
                IdProduct = detail.IdProduct,
                Quantity = detail.Quantity
            });
        }

        _db.Resupplies.Add(entity);
        await _db.SaveChangesAsync();

        var created = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(r => r.IdResupply == entity.IdResupply);

        return (MapToResponse(created), null);
    }

    public async Task<(ResupplyResponse? Result, string? Error)> CompleteAsync(int id)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var entity = await _db.Resupplies
            .Include(r => r.ResupplyDetails)
            .FirstOrDefaultAsync(r => r.IdResupply == id);

        if (entity is null)
            return (null, "Reposición no encontrada.");

        if (!string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return (null, "Solo se pueden completar reposiciones en estado Pending.");

        foreach (var detail in entity.ResupplyDetails)
        {
            var inventory = await _db.Inventories.FirstOrDefaultAsync(i =>
                i.IdPointSale == entity.IdPointSale &&
                i.IdProduct == detail.IdProduct);

            if (inventory is null)
            {
                inventory = new Inventory
                {
                    IdPointSale = entity.IdPointSale,
                    IdProduct = detail.IdProduct,
                    UnitsAvailable = 0,
                    MinStock = 0,
                    OpenUnitRemainingDoses = 0
                };
                _db.Inventories.Add(inventory);
            }

            inventory.UnitsAvailable += detail.Quantity;
        }

        entity.Status = "Completed";
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var completed = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(r => r.IdResupply == id);

        return (MapToResponse(completed), null);
    }

    public async Task<(ResupplyResponse? Result, string? Error)> CancelAsync(int id)
    {
        var entity = await _db.Resupplies.FirstOrDefaultAsync(r => r.IdResupply == id);
        if (entity is null)
            return (null, "Reposición no encontrada.");

        if (!string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return (null, "Solo se pueden cancelar reposiciones en estado Pending.");

        entity.Status = "Cancelled";
        await _db.SaveChangesAsync();

        var cancelled = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(r => r.IdResupply == id);

        return (MapToResponse(cancelled), null);
    }
}
