using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Bill;

namespace TheFarm.API.Services;

public class BillService
{
    private readonly AppDbContext _db;

    public BillService(AppDbContext db)
    {
        _db = db;
    }

    private static BillResponse MapToResponse(Bill b) => new()
    {
        IdBill = b.IdBill,
        IdClient = b.IdClient,
        ClientName = b.Client is null ? null : $"{b.Client.Name} {b.Client.LastName}".Trim(),
        ClientEmail = b.Client?.Email,
        IdSale = b.IdSale,
        SaleTotal = b.Sale?.Total,
        SaleDate = b.Sale?.Date,
        EmailStatus = b.EmailStatus,
        EmailSendAt = b.EmailSendAt
    };

    private IQueryable<Bill> BaseQuery() =>
        _db.Bills
            .Include(b => b.Client)
            .Include(b => b.Sale);

    public async Task<List<BillResponse>> GetAllAsync(int? idSale, int? idClient)
    {
        var query = BaseQuery().AsNoTracking();

        if (idSale.HasValue)
            query = query.Where(b => b.IdSale == idSale.Value);

        if (idClient.HasValue)
            query = query.Where(b => b.IdClient == idClient.Value);

        var items = await query
            .OrderByDescending(b => b.IdBill)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<BillResponse?> GetByIdAsync(int id)
    {
        var item = await BaseQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdBill == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(BillResponse? Result, string? Error)> CreateAsync(CreateBillRequest request)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.IdClient == request.IdClient);
        if (!clientExists)
            return (null, "El cliente no existe.");

        var saleExists = await _db.Sales.AnyAsync(s => s.IdSale == request.IdSale);
        if (!saleExists)
            return (null, "La venta no existe.");

        var billExists = await _db.Bills.AnyAsync(b => b.IdSale == request.IdSale);
        if (billExists)
            return (null, "Ya existe una factura para esa venta.");

        var entity = new Bill
        {
            IdClient = request.IdClient,
            IdSale = request.IdSale,
            EmailStatus = "Pending",
            EmailSendAt = null
        };

        _db.Bills.Add(entity);
        await _db.SaveChangesAsync();

        var created = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(b => b.IdBill == entity.IdBill);

        return (MapToResponse(created), null);
    }

    public async Task<(BillResponse? Result, string? Error)> MarkSentAsync(int id)
    {
        var entity = await _db.Bills.FirstOrDefaultAsync(b => b.IdBill == id);
        if (entity is null)
            return (null, "Factura no encontrada.");

        entity.EmailStatus = "Sent";
        entity.EmailSendAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var updated = await BaseQuery()
            .AsNoTracking()
            .FirstAsync(b => b.IdBill == id);

        return (MapToResponse(updated), null);
    }
}
