using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.PointSale;

namespace TheFarm.API.Services;

public class PointSaleService
{
    private readonly AppDbContext _db;

    public PointSaleService(AppDbContext db)
    {
        _db = db;
    }

    private static PointSaleResponse MapToResponse(PointSale p)
    {
        return new PointSaleResponse
        {
            IdPointSale = p.IdPointSale,
            Name = p.Name,
            Adress = p.Adress,
            PhoneNumber = p.PhoneNumber,
            Status = p.Status,
            IdUser = p.IdUser,
            ManagerUserName = p.User?.Username,
            ManagerName = p.User is null ? null : $"{p.User.Name} {p.User.LastName}"
        };
    }

    public async Task<List<PointSaleResponse>> GetAllAsync()
    {
        var items = await _db.PointSales
            .AsNoTracking()
            .Include(p => p.User)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<PointSaleResponse?> GetByIdAsync(int id)
    {
        var item = await _db.PointSales
            .AsNoTracking()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.IdPointSale == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(PointSaleResponse? Result, string? Error)> CreateAsync(CreatePointSaleRequest request)
    {
        var manager = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUser == request.IdUser);


        if (manager is null)
            return (null, "El encargado no existe");

        var alreadyHasPos = await _db.PointSales 
            .AnyAsync(p => p.IdUser == request.IdUser && p.Status);

        if (alreadyHasPos)
            return (null, "Ese usuario ya tiene un punto de venta asociado");

        var entity = new PointSale
        {
            Name = request.Name,
            Adress = request.Adress,
            PhoneNumber = request.PhoneNumber,
            IdUser = request.IdUser,
            Status = true
        };

        _db.PointSales.Add(entity);
        await _db.SaveChangesAsync();

        var created = await _db.PointSales
        .AsNoTracking()
        .Include(p => p.User)
        .FirstAsync(p => p.IdPointSale == entity.IdPointSale);

        return (MapToResponse(created), null);
    }

    public async Task<(PointSaleResponse? Result, string? Error)> UpdateAsync(int id, UpdatePointSaleRequest request)
    {
        var entity = await _db.PointSales
            .FirstOrDefaultAsync(p => p.IdPointSale == id);

        if (entity is null)
            return (null, "El punto de venta no existe");
        
        if (request.IdUser != entity.IdUser)
        {
            var managerExists = await _db.Users
                .AnyAsync(u => u.IdUser == request.IdUser);

            if (!managerExists)
                return (null, "El encargado no existe");

            var taken = await _db.PointSales
            .AnyAsync(p => p.IdUser == request.IdUser && p.IdPointSale != id);
        
            if (taken)
                return (null, "Ese usuario ya tiene un punto de venta asociado");
        }

        entity.Name = request.Name;
        entity.Adress = request.Adress;
        entity.PhoneNumber = request.PhoneNumber;
        entity.IdUser = request.IdUser;
        entity.Status = request.Status;

        
        await _db.SaveChangesAsync();

        var updated = await _db.PointSales
            .AsNoTracking()
            .Include(p => p.User)
            .FirstAsync(p => p.IdPointSale == id);

        return (MapToResponse(updated), null);
    }
}
