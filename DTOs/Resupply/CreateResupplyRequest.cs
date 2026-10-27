namespace TheFarm.API.DTOs.Resupply;

public class CreateResupplyRequest
{
    public int IdPointSale { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<CreateResupplyDetailRequest> Details { get; set; } = new();
}
