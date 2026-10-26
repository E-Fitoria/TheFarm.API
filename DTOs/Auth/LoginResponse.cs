namespace TheFarm.API.DTOs.Auth;

public class LoginResponse
{
    public string Token {get; set;} = string.Empty;
    public int IdUser {get; set;}
    public string Username {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
    public DateTime ExpireAt {get; set;}
}