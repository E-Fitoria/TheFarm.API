namespace TheFarm.API.DTOs.Sale;

public class CreateSaleRequest
{
    public int IdPointSale { get; set; }
    public List<CreateSaleDetailRequest> Details { get; set; } = new();
}
