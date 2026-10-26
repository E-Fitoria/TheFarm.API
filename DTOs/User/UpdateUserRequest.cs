namespace TheFarm.API.DTOs.User;

public class UpdateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Status { get; set; }
    public string Role { get; set; } = string.Empty;

    public string? Password { get; set; }
}