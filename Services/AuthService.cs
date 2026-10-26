using Microsoft.EntityFrameworkCore;
using TheFarm.API.Auth;
using TheFarm.API.Data;
using TheFarm.API.DTOs.Auth;

namespace TheFarm.API.Services;

public class AuthService
{

    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthService(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // buscar usuario por username
        var user = await _db.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null) return null;

        // solo usuarios activos
        if (!user.Status) return null;

        // Password en BD debe ser hash por BCrypt.Net-Next
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            return null;

        // crear token JWT
        var (token, expiresAt) = _jwt.CreateToken(user);

        return new LoginResponse
        {
            Token = token,
            IdUser = user.IdUser,
            Username = user.Username,
            Role = user.Role,
            ExpireAt = expiresAt
        };
    }
}