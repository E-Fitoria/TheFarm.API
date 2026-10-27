using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Inventory;

namespace TheFarm.API.Services;

public class InventoryService
{
    private readonly AppDbContext _db;

    public InventoryService(AppDbContext db)
    {
        _db = db;
    }

    private static InventoryResponse MapToResponse(Inventory i) => new()
    {
        IdInventory = i.IdInventory,
        IdPointSale = i.IdPointSale,
        PointSaleName = i.PointSale?.Name,
        IdProduct = i.IdProduct,
        ProductName = i.Product?.Name,
        UnitsAvailable = i.UnitsAvailable,
        MinStock = i.MinStock,
        OpenUnitRemainingDoses = i.OpenUnitRemainingDoses,
        IsLowStock = i.UnitsAvailable <= i.MinStock
    };

    public async Task<List<InventoryResponse>> GetAllAsync(int? idPointSale, int? idProduct)
    {
        var query = _db.Inventories
            .AsNoTracking()
            .Include(i => i.PointSale)
            .Include(i => i.Product)
            .AsQueryable();

        if (idPointSale.HasValue)
            query = query.Where(i => i.IdPointSale == idPointSale.Value);

        if (idProduct.HasValue)
            query = query.Where(i => i.IdProduct == idProduct.Value);

        var items = await query
            .OrderBy(i => i.IdPointSale)
            .ThenBy(i => i.Product!.Name)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<InventoryResponse?> GetByIdAsync(int id)
    {
        var item = await _db.Inventories
            .AsNoTracking()
            .Include(i => i.PointSale)
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.IdInventory == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(InventoryResponse? Result, string? Error)> CreateAsync(CreateInventoryRequest request)
    {
        if (request.UnitsAvailable < 0 || request.MinStock < 0 || request.OpenUnitRemainingDoses < 0)
            return (null, "Las cantidades no pueden ser negativas.");

        var posExists = await _db.PointSales.AnyAsync(p => p.IdPointSale == request.IdPointSale);
        if (!posExists)
            return (null, "El punto de venta no existe.");

        var productExists = await _db.Products.AnyAsync(p => p.IdProduct == request.IdProduct);
        if (!productExists)
            return (null, "El producto no existe.");

        // Stock independiente por sucursal: no duplicar el mismo producto en el mismo POS
        var alreadyExists = await _db.Inventories.AnyAsync(i =>
            i.IdPointSale == request.IdPointSale && i.IdProduct == request.IdProduct);
        if (alreadyExists)
            return (null, "Ya existe inventario de ese producto en ese punto de venta.");

        var entity = new Inventory
        {
            IdPointSale = request.IdPointSale,
            IdProduct = request.IdProduct,
            UnitsAvailable = request.UnitsAvailable,
            MinStock = request.MinStock,
            OpenUnitRemainingDoses = request.OpenUnitRemainingDoses
        };

        _db.Inventories.Add(entity);
        await _db.SaveChangesAsync();

        var created = await _db.Inventories
            .AsNoTracking()
            .Include(i => i.PointSale)
            .Include(i => i.Product)
            .FirstAsync(i => i.IdInventory == entity.IdInventory);

        return (MapToResponse(created), null);
    }

    public async Task<(InventoryResponse? Result, string? Error)> UpdateAsync(int id, UpdateInventoryRequest request)
    {
        var entity = await _db.Inventories.FirstOrDefaultAsync(i => i.IdInventory == id);
        if (entity is null)
            return (null, "Inventario no encontrado.");

        if (request.UnitsAvailable < 0 || request.MinStock < 0 || request.OpenUnitRemainingDoses < 0)
            return (null, "Las cantidades no pueden ser negativas.");

        entity.UnitsAvailable = request.UnitsAvailable;
        entity.MinStock = request.MinStock;
        entity.OpenUnitRemainingDoses = request.OpenUnitRemainingDoses;

        await _db.SaveChangesAsync();

        var updated = await _db.Inventories
            .AsNoTracking()
            .Include(i => i.PointSale)
            .Include(i => i.Product)
            .FirstAsync(i => i.IdInventory == id);

        return (MapToResponse(updated), null);
    }
}