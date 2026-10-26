using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.User;

namespace TheFarm.API.Services;

public class UserService
{
    private readonly AppDbContext _db;

    public static readonly HashSet<string> AllowedRoles =
        new(StringComparer.OrdinalIgnoreCase) { "ADMIN", "SELLER" };

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    private static UserResponse MapToResponse(User u) => new()
    {
        IdUser = u.IdUser,
        Name = u.Name,
        LastName = u.LastName,
        Email = u.Email,
        Username = u.Username,
        Role = u.Role,
        Status = u.Status,
        IdPointSale = u.PointSale?.IdPointSale,
        PointSaleName = u.PointSale?.Name
        // Password nunca se expone
    };

    private static string? NormalizeRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role) || !AllowedRoles.Contains(role))
            return null;

        return role.Trim().ToUpperInvariant();
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _db.Users
            .AsNoTracking()
            .Include(u => u.PointSale)
            .OrderBy(u => u.Name)
            .ToListAsync();

        return users.Select(MapToResponse).ToList();
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.PointSale)
            .FirstOrDefaultAsync(u => u.IdUser == id);

        return user is null ? null : MapToResponse(user);
    }

    public async Task<(UserResponse? Result, string? Error)> CreateAsync(CreateUserRequest request)
    {
        var role = NormalizeRole(request.Role);
        if (role is null)
            return (null, "Rol inválido. Use ADMIN o SELLER");

        var usernameTaken = await _db.Users
            .AnyAsync(u => u.Username == request.Username);

        if (usernameTaken)
            return (null, "El nombre de usuario ya está en uso");

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == request.Email);

        if (emailTaken)
            return (null, "El correo ya está en uso");

        var entity = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            Username = request.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
            Status = true
        };

        _db.Users.Add(entity);
        await _db.SaveChangesAsync();

        var created = await _db.Users
            .AsNoTracking()
            .Include(u => u.PointSale)
            .FirstAsync(u => u.IdUser == entity.IdUser);

        return (MapToResponse(created), null);
    }

    public async Task<(UserResponse? Result, string? Error)> UpdateAsync(int id, UpdateUserRequest request)
    {
        var entity = await _db.Users
            .FirstOrDefaultAsync(u => u.IdUser == id);

        if (entity is null)
            return (null, "El usuario no existe");

        var role = NormalizeRole(request.Role);
        if (role is null)
            return (null, "Rol inválido. Use ADMIN o SELLER");

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == request.Email && u.IdUser != id);

        if (emailTaken)
            return (null, "El correo ya está en uso");

        entity.Name = request.Name;
        entity.LastName = request.LastName;
        entity.Email = request.Email;
        entity.Role = role;
        entity.Status = request.Status;

        if (!string.IsNullOrWhiteSpace(request.Password))
            entity.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        await _db.SaveChangesAsync();

        var updated = await _db.Users
            .AsNoTracking()
            .Include(u => u.PointSale)
            .FirstAsync(u => u.IdUser == id);

        return (MapToResponse(updated), null);
    }
}
