namespace TheFarm.API.DTOs.InventoryTransfer;

public class CreateInventoryTransferRequest
{
    public string Description { get; set; } = string.Empty;
    public int IdOriginatingPointSale { get; set; }
    public int IdDestinationPointSale { get; set; }
    public List<CreateInventoryTransferDetailRequest> Details { get; set; } = new ();
}