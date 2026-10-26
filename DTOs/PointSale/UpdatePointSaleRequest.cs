namespace TheFarm.API.DTOs.PointSale;

public class UpdatePointSaleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Status { get; set; }
    public int IdUser { get; set; }
}