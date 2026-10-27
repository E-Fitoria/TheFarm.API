namespace TheFarm.API.DTOs.Inventory;

public class CreateInventoryRequest
{
    public int IdPointSale { get; set; }
    public int IdProduct { get; set; }
    public int UnitsAvailable { get; set; }
    public int MinStock { get; set; }
    public int OpenUnitRemainingDoses { get; set; }
}