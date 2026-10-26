namespace TheFarm.API.Domain.Entities;

public class InventoryTransferDetail
{
    public int IdInventoryTransferDetail {get; set;}
    public int ProductQuantity {get; set;}
    public int IdInventoryTransfer {get; set;}
    public int IdProduct {get; set;}

    public InventoryTransfer? InventoryTransfer {get; set;}
    public Product? Product {get; set;}
}