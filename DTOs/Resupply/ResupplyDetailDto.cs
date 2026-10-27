namespace TheFarm.API.DTOs.Resupply;

public class ResupplyDetailDto
{
    public int IdResupplyDetail { get; set; }
    public int IdProduct { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}
