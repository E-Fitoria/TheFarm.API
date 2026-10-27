namespace TheFarm.API.DTOs.Sale;

public class CreateSaleDetailRequest
{
    public int IdProduct { get; set; }
    public int Quantity { get; set; }
    /// <summary>"Unit" o "Dose"</summary>
    public string SaleType { get; set; } = "Unit";
}
