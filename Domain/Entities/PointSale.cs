namespace TheFarm.API.Domain.Entities;

public class PointSale
{
    public int IdPointSale { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Status { get; set; }
    public int IdUser { get; set; }

    public User? User { get; set; }
}
