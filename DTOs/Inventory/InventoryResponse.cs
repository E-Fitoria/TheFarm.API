namespace TheFarm.API.DTOs.Inventory;

public class InventoryResponse
{
    public int IdInventory { get; set; }
    public int IdPointSale { get; set; }
    public string? PointSaleName { get; set; } 
    public int IdProduct { get; set; }
    public string? ProductName { get; set; } 
    public int UnitsAvailable { get; set; }
    public int MinStock { get; set; }
    public int OpenUnitRemainingDoses { get; set; }
    public bool IsLowStock { get; set; }
}