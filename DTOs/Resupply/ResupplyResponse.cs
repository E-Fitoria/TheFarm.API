namespace TheFarm.API.DTOs.Resupply;

public class ResupplyResponse
{
    public int IdResupply { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int IdPointSale { get; set; }
    public string? PointSaleName { get; set; }
    public int IdUser { get; set; }
    public string? Username { get; set; }
    public List<ResupplyDetailDto> Details { get; set; } = new();
}
