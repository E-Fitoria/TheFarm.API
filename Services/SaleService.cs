using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Sale;

namespace TheFarm.API.Services;

public class SaleService
{
    private readonly AppDbContext _db;

    public SaleService(AppDbContext db)
    {
        _db = db;
    }

    private static SaleResponse MapToResponse(Sale s) => new()
    {
        IdSale = s.IdSale,
        Date = s.Date,
        Total = s.Total,
        IdUser = s.IdUser,
        Username = s.User?.Username,
        IdPointSale = s.IdPointSale,
        PointSaleName = s.PointSale?.Name,
        Details = s.SaleDetails.Select(d => new SaleDetailDto
        {
            IdSaleDetail = d.IdSaleDetail,
            IdProduct = d.IdProduct,
            ProductName = d.Product?.Name,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            SaleType = d.SaleType
        }).ToList()
    };

    private IQueryable<Sale> BaseQuery() =>
        _db.Sales
            .Include(s => s.User)
            .Include(s => s.PointSale)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Product);

    public async Task<List<SaleResponse>> GetAllAsync(int? idPointSale, int? idUser)
    {
        var query = BaseQuery().AsNoTracking();

        if (idPointSale.HasValue)
            query = query.Where(s => s.IdPointSale == idPointSale.Value);

        if (idUser.HasValue)
            query = query.Where(s => s.IdUser == idUser.Value);

        var items = await query
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<SaleResponse?> GetByIdAsync(int id)
    {
        var item = await BaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdSale == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(SaleResponse? Result, string? Error)> CreateAsync(
        CreateSaleRequest request,
        int sellingUserId)
    {
        if (sellingUserId <= 0)
            return (null, "Usuario vendedor inválido.");

        if (request.Details is null || request.Details.Count == 0)
            return (null, "Debe indicar al menos un detalle de venta.");

        if (request.Details.Any(d => d.Quantity <= 0))
            return (null, "Las cantidades deben ser mayores que cero.");

        var posExists = await _db.PointSales.AnyAsync(p => p.IdPointSale == request.IdPointSale);
        if (!posExists)
            return (null, "El punto de venta no existe.");

        var userExists = await _db.Users.AnyAsync(u => u.IdUser == sellingUserId);
        if (!userExists)
            return (null, "El usuario vendedor no existe.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var saleDetails = new List<SaleDetail>();
        decimal total = 0;

        foreach (var line in request.Details)
        {
            var saleType = NormalizeSaleType(line.SaleType);
            if (saleType is null)
                return (null, $"Tipo de venta inválido: '{line.SaleType}'. Use 'Unit' o 'Dose'.");

            var product = await _db.Products.FirstOrDefaultAsync(p => p.IdProduct == line.IdProduct);
            if (product is null)
                return (null, $"El producto {line.IdProduct} no existe.");

            decimal unitPrice;
            if (saleType == "Unit")
            {
                unitPrice = product.SalePrice;
            }
            else
            {
                if (!product.AllowsDoseSale)
                    return (null, $"El producto '{product.Name}' no permite venta por dosis.");
                if (product.ServicesNumber <= 0)
                    return (null, $"El producto '{product.Name}' no tiene número de servicios/dosis configurado.");
                unitPrice = product.DoseSalePrice;
            }

            var inventory = await _db.Inventories.FirstOrDefaultAsync(i =>
                i.IdPointSale == request.IdPointSale &&
                i.IdProduct == line.IdProduct);

            if (inventory is null)
                return (null, $"No hay inventario del producto '{product.Name}' en ese punto de venta.");

            var stockError = ApplyStockDecrement(inventory, product, saleType, line.Quantity);
            if (stockError is not null)
                return (null, stockError);

            total += line.Quantity * unitPrice;
            saleDetails.Add(new SaleDetail
            {
                IdProduct = line.IdProduct,
                Quantity = line.Quantity,
                UnitPrice = unitPrice,
                SaleType = saleType
            });
        }

        var sale = new Sale
        {
            Date = DateTime.UtcNow,
            Total = total,
            IdUser = sellingUserId,
            IdPointSale = request.IdPointSale
        };

        foreach (var detail in saleDetails)
            sale.SaleDetails.Add(detail);

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var created = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(s => s.IdSale == sale.IdSale);

        return (MapToResponse(created), null);
    }

    /// <summary>
    /// Unit: resta UnitsAvailable.
    /// Dose: resta OpenUnitRemainingDoses; si no alcanzan, abre una unidad cerrada
    /// (UnitsAvailable - 1 y OpenUnitRemainingDoses += ServicesNumber) y continúa.
    /// </summary>
    private static string? ApplyStockDecrement(
        Inventory inventory,
        Product product,
        string saleType,
        int quantity)
    {
        if (saleType == "Unit")
        {
            if (inventory.UnitsAvailable < quantity)
                return $"Stock insuficiente de '{product.Name}'. Disponibles: {inventory.UnitsAvailable}, solicitados: {quantity}.";

            inventory.UnitsAvailable -= quantity;
            return null;
        }

        // Dose
        var remainingToSell = quantity;

        while (remainingToSell > 0)
        {
            if (inventory.OpenUnitRemainingDoses > 0)
            {
                var take = Math.Min(inventory.OpenUnitRemainingDoses, remainingToSell);
                inventory.OpenUnitRemainingDoses -= take;
                remainingToSell -= take;
                continue;
            }

            // Abrir una unidad cerrada para obtener dosis
            if (inventory.UnitsAvailable <= 0)
                return $"Dosis insuficientes de '{product.Name}'. No hay unidades cerradas ni dosis abiertas disponibles.";

            inventory.UnitsAvailable -= 1;
            inventory.OpenUnitRemainingDoses += product.ServicesNumber;
        }

        return null;
    }

    private static string? NormalizeSaleType(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var value = raw.Trim();
        if (value.Equals("Unit", StringComparison.OrdinalIgnoreCase))
            return "Unit";
        if (value.Equals("Dose", StringComparison.OrdinalIgnoreCase))
            return "Dose";
        return null;
    }
}
