namespace TheFarm.API.DTOs.Client;

public class ClientResponse
{
    public int IdClient { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
