namespace TheFarm.API.Domain.Entities;

public class Client
{
    public int IdClient {get; set;}
    public string Name {get; set;} = string.Empty;
    public string LastName {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
}
