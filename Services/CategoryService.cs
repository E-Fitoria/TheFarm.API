using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Category;

namespace TheFarm.API.Services;

public class CategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    private static CategoryResponse MapToResponse(Category c) => new()
    {
        IdCategory = c.IdCategory,
        Name = c.Name,
        Description = c.Description,
        Status = c.Status
    };

    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        var items = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var item = await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategory == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(CategoryResponse? Result, string? Error)> CreateAsync(CreateCategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (null, "El nombre es obligatorio.");

        var entity = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Status = true
        };

        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();

        return (MapToResponse(entity), null);
    }

    public async Task<(CategoryResponse? Result, string? Error)> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.IdCategory == id);
        if (entity is null)
            return (null, "Categoría no encontrada.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return (null, "El nombre es obligatorio.");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.Status = request.Status;

        await _db.SaveChangesAsync();
        return (MapToResponse(entity), null);
    }
}