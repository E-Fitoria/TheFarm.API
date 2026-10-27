namespace TheFarm.API.DTOs.Product;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool AllowsDoseSale { get; set; }
    public int IdCategory { get; set; }
    public decimal SalePrice { get; set; }
    public decimal DoseSalePrice { get; set; }
    public int ServicesNumber { get; set; }
}