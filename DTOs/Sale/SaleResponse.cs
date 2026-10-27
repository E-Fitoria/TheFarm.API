namespace TheFarm.API.DTOs.Sale;

public class SaleResponse
{
    public int IdSale { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public int IdUser { get; set; }
    public string? Username { get; set; }
    public int IdPointSale { get; set; }
    public string? PointSaleName { get; set; }
    public List<SaleDetailDto> Details { get; set; } = new();
}
