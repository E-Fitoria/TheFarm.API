namespace TheFarm.API.DTOs.InventoryTransfer;

public class CreateInventoryTransferDetailRequest
{
    public int IdProduct { get; set; }
    public int ProductQuantity { get; set; }
}