namespace TheFarm.API.Domain.Entities;

public class User
{
    public int IdUser { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool Status { get; set; } 

    // propiedad de navegacion para la relacion con la tabla PointSale
    public PointSale? PointSale { get; set; }
}