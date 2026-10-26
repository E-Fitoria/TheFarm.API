namespace TheFarm.API.DTOs.User;

// DTO con la informacion que se muestra en el frontend del usuario
public class UserResponse 
{
    public int IdUser { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Status { get; set; }

    // relacion con el punto de venta
    public int? IdPointSale { get; set; }
    public string? PointSaleName { get; set; }
}