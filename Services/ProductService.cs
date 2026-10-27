using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Product;

namespace TheFarm.API.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    private static ProductResponse MapToResponse(Product p) => new()
    {
        IdProduct = p.IdProduct,
        Name = p.Name,
        Description = p.Description,
        AllowsDoseSale = p.AllowsDoseSale,
        IdCategory = p.IdCategory,
        CategoryName = p.Category?.Name,
        SalePrice = p.SalePrice,
        DoseSalePrice = p.DoseSalePrice,
        ServicesNumber = p.ServicesNumber
    };

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        var items = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var item = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.IdProduct == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(ProductResponse? Result, string? Error)> CreateAsync(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (null, "El nombre es obligatorio.");

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.IdCategory == request.IdCategory);
        if (!categoryExists)
            return (null, "La categoría no existe.");

        var entity = new Product
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            AllowsDoseSale = request.AllowsDoseSale,
            IdCategory = request.IdCategory,
            SalePrice = request.SalePrice,
            DoseSalePrice = request.AllowsDoseSale ? request.DoseSalePrice : 0,
            ServicesNumber = request.ServicesNumber
        };

        _db.Products.Add(entity);

        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\proyectos-uni\the-farm\debug-94471a.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "94471a", runId = "post-fix", hypothesisId = "H1", location = "ProductService.CreateAsync:beforeSave", message = "Saving Product without Status", data = new { entity.Name, entity.IdCategory, entity.ServicesNumber }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion

        await _db.SaveChangesAsync();

        // #region agent log
        try { System.IO.File.AppendAllText(@"c:\proyectos-uni\the-farm\debug-94471a.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "94471a", runId = "post-fix", hypothesisId = "H1", location = "ProductService.CreateAsync:afterSave", message = "Product saved OK", data = new { entity.IdProduct }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
        // #endregion

        var created = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstAsync(p => p.IdProduct == entity.IdProduct);

        return (MapToResponse(created), null);
    }

    public async Task<(ProductResponse? Result, string? Error)> UpdateAsync(int id, UpdateProductRequest request)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.IdProduct == id);
        if (entity is null)
            return (null, "Producto no encontrado.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return (null, "El nombre es obligatorio.");

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.IdCategory == request.IdCategory);
        if (!categoryExists)
            return (null, "La categoría no existe.");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.AllowsDoseSale = request.AllowsDoseSale;
        entity.IdCategory = request.IdCategory;
        entity.SalePrice = request.SalePrice;
        entity.DoseSalePrice = request.AllowsDoseSale ? request.DoseSalePrice : 0;
        entity.ServicesNumber = request.ServicesNumber;

        await _db.SaveChangesAsync();

        var updated = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstAsync(p => p.IdProduct == id);

        return (MapToResponse(updated), null);
    }
}
