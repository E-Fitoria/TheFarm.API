using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.InventoryTransfer;

namespace TheFarm.API.Services;

public class InventoryTransferService
{
    private readonly AppDbContext _db;

    public InventoryTransferService(AppDbContext db)
    {
        _db = db;
    }

    private static InventoryTransferResponse MapToResponse(InventoryTransfer t) => new()
    {
        IdInventoryTransfer = t.IdInventoryTransfer,
        ApplicationDate = t.ApplicationDate,
        ApprovalDate = t.ApprovalDate,
        Status = t.Status,
        Description = t.Description,
        IdOriginatingPointSale = t.IdOriginatingPointSale,
        OriginatingPointSaleName = t.OriginatingPointSale?.Name,
        IdDestinationPointSale = t.IdDestinationPointSale,
        DestinationPointSaleName = t.DestinationPointSale?.Name,
        IdRequestingUser = t.IdRequestingUser,
        RequestingUsername = t.RequestingUser?.Username,
        IdApprovingUser = t.IdApprovingUser,
        ApprovingUsername = t.ApprovingUser?.Username,
        Details = t.InventoryTransferDetails.Select(d => new InventoryTransferDetailDto
        {
            IdProduct = d.IdProduct,
            ProductName = d.Product?.Name,
            ProductQuantity = d.ProductQuantity
        }).ToList()
    };

    private IQueryable<InventoryTransfer> BaseQuery() =>
        _db.InventoryTransfers
            .Include(t => t.OriginatingPointSale)
            .Include(t => t.DestinationPointSale)
            .Include(t => t.RequestingUser)
            .Include(t => t.ApprovingUser)
            .Include(t => t.InventoryTransferDetails)
                .ThenInclude(d => d.Product);

    public async Task<List<InventoryTransferResponse>> GetAllAsync(string? status)
    {
        var query = BaseQuery().AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status.Trim());

        var items = await query
            .OrderByDescending(t => t.ApplicationDate)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<InventoryTransferResponse?> GetByIdAsync(int id)
    {
        var item = await BaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IdInventoryTransfer == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(InventoryTransferResponse? Result, string? Error)> CreateAsync(
        CreateInventoryTransferRequest request,
        int requestingUserId)
    {
        if (requestingUserId <= 0)
            return (null, "Usuario solicitante inválido.");

        if (request.IdOriginatingPointSale == request.IdDestinationPointSale)
            return (null, "El origen y el destino deben ser puntos de venta distintos.");

        if (request.Details is null || request.Details.Count == 0)
            return (null, "Debe indicar al menos un producto en la transferencia.");

        if (request.Details.Any(d => d.ProductQuantity <= 0))
            return (null, "Las cantidades deben ser mayores que cero.");

        var originExists = await _db.PointSales.AnyAsync(p => p.IdPointSale == request.IdOriginatingPointSale);
        var destExists = await _db.PointSales.AnyAsync(p => p.IdPointSale == request.IdDestinationPointSale);
        if (!originExists || !destExists)
            return (null, "Punto de venta origen o destino no existe.");

        var userExists = await _db.Users.AnyAsync(u => u.IdUser == requestingUserId);
        if (!userExists)
            return (null, "El usuario solicitante no existe.");

        foreach (var detail in request.Details)
        {
            var productExists = await _db.Products.AnyAsync(p => p.IdProduct == detail.IdProduct);
            if (!productExists)
                return (null, $"El producto {detail.IdProduct} no existe.");
        }

        var entity = new InventoryTransfer
        {
            ApplicationDate = DateTime.UtcNow,
            ApprovalDate = null,
            Status = "Pending",
            Description = request.Description?.Trim() ?? string.Empty,
            IdOriginatingPointSale = request.IdOriginatingPointSale,
            IdDestinationPointSale = request.IdDestinationPointSale,
            IdRequestingUser = requestingUserId,
            IdApprovingUser = null
        };

        foreach (var detail in request.Details)
        {
            entity.InventoryTransferDetails.Add(new InventoryTransferDetail
            {
                IdProduct = detail.IdProduct,
                ProductQuantity = detail.ProductQuantity
            });
        }

        _db.InventoryTransfers.Add(entity);
        await _db.SaveChangesAsync();

        var created = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(t => t.IdInventoryTransfer == entity.IdInventoryTransfer);

        return (MapToResponse(created), null);
    }

    public async Task<(InventoryTransferResponse? Result, string? Error)> DecideAsync(
        int id,
        DecideTransferRequest request,
        int approvingUserId)
    {
        if (approvingUserId <= 0)
            return (null, "Usuario aprobador inválido.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var entity = await _db.InventoryTransfers
            .Include(t => t.InventoryTransferDetails)
            .FirstOrDefaultAsync(t => t.IdInventoryTransfer == id);

        if (entity is null)
            return (null, "Transferencia no encontrada.");

        if (!string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
            return (null, "Solo se pueden decidir transferencias en estado Pending.");

        if (!request.Approve)
        {
            entity.Status = "Rejected";
            entity.ApprovalDate = DateTime.UtcNow;
            entity.IdApprovingUser = approvingUserId;
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            var rejected = await BaseQuery()
                .AsNoTracking()
                .FirstAsync(t => t.IdInventoryTransfer == id);
            return (MapToResponse(rejected), null);
        }

        // Approve: mover stock entre inventarios de cada POS
        foreach (var detail in entity.InventoryTransferDetails)
        {
            var originInv = await _db.Inventories.FirstOrDefaultAsync(i =>
                i.IdPointSale == entity.IdOriginatingPointSale &&
                i.IdProduct == detail.IdProduct);

            if (originInv is null)
                return (null, $"No hay inventario del producto {detail.IdProduct} en el punto de venta origen.");

            if (originInv.UnitsAvailable < detail.ProductQuantity)
                return (null, $"Stock insuficiente del producto {detail.IdProduct} en el origen.");

            var destInv = await _db.Inventories.FirstOrDefaultAsync(i =>
                i.IdPointSale == entity.IdDestinationPointSale &&
                i.IdProduct == detail.IdProduct);

            if (destInv is null)
            {
                destInv = new Inventory
                {
                    IdPointSale = entity.IdDestinationPointSale,
                    IdProduct = detail.IdProduct,
                    UnitsAvailable = 0,
                    MinStock = originInv.MinStock,
                    OpenUnitRemainingDoses = 0
                };
                _db.Inventories.Add(destInv);
            }

            originInv.UnitsAvailable -= detail.ProductQuantity;
            destInv.UnitsAvailable += detail.ProductQuantity;
        }

        entity.Status = "Approved";
        entity.ApprovalDate = DateTime.UtcNow;
        entity.IdApprovingUser = approvingUserId;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var approved = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(t => t.IdInventoryTransfer == id);

        return (MapToResponse(approved), null);
    }
}