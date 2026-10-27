namespace TheFarm.API.DTOs.Inventory;

public class UpdateInventoryRequest
{
    public int UnitsAvailable { get; set; }
    public int MinStock { get; set; }
    public int OpenUnitRemainingDoses { get; set; }
}