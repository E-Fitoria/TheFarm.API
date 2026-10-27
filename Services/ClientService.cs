using Microsoft.EntityFrameworkCore;
using TheFarm.API.Data;
using TheFarm.API.Domain.Entities;
using TheFarm.API.DTOs.Client;

namespace TheFarm.API.Services;

public class ClientService
{
    private readonly AppDbContext _db;

    public ClientService(AppDbContext db)
    {
        _db = db;
    }

    private static ClientResponse MapToResponse(Client c) => new()
    {
        IdClient = c.IdClient,
        Name = c.Name,
        LastName = c.LastName,
        Email = c.Email
    };

    public async Task<List<ClientResponse>> GetAllAsync()
    {
        var items = await _db.Clients
            .AsNoTracking()
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return items.Select(MapToResponse).ToList();
    }

    public async Task<ClientResponse?> GetByIdAsync(int id)
    {
        var item = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdClient == id);

        return item is null ? null : MapToResponse(item);
    }

    public async Task<(ClientResponse? Result, string? Error)> CreateAsync(CreateClientRequest request)
    {
        var validationError = Validate(request.Name, request.LastName, request.Email);
        if (validationError is not null)
            return (null, validationError);

        var entity = new Client
        {
            Name = request.Name.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim()
        };

        _db.Clients.Add(entity);
        await _db.SaveChangesAsync();

        return (MapToResponse(entity), null);
    }

    public async Task<(ClientResponse? Result, string? Error)> UpdateAsync(int id, UpdateClientRequest request)
    {
        var entity = await _db.Clients.FirstOrDefaultAsync(c => c.IdClient == id);
        if (entity is null)
            return (null, "Cliente no encontrado.");

        var validationError = Validate(request.Name, request.LastName, request.Email);
        if (validationError is not null)
            return (null, validationError);

        entity.Name = request.Name.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Email = request.Email.Trim();

        await _db.SaveChangesAsync();
        return (MapToResponse(entity), null);
    }

    private static string? Validate(string name, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "El nombre es obligatorio.";
        if (string.IsNullOrWhiteSpace(lastName))
            return "El apellido es obligatorio.";
        if (string.IsNullOrWhiteSpace(email))
            return "El email es obligatorio.";
        if (!email.Contains('@'))
            return "El email no es válido.";
        return null;
    }
}
