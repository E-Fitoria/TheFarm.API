namespace TheFarm.API.DTOs.Sale;

public class SaleDetailDto
{
    public int IdSaleDetail { get; set; }
    public int IdProduct { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string SaleType { get; set; } = string.Empty;
}
