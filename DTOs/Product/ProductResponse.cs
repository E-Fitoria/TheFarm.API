namespace TheFarm.API.DTOs.Product;

public class ProductResponse
{
    public int IdProduct { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool AllowsDoseSale { get; set; }
    public int IdCategory { get; set; }
    public string? CategoryName { get; set; } = string.Empty;
    public decimal? SalePrice { get; set; }
    public decimal DoseSalePrice { get; set; }
    public int ServicesNumber { get; set; }
}