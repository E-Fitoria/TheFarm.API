namespace TheFarm.API.Domain.Entities;

public class InventoryTransfer
{
    public int IdInventoryTransfer {get; set;}
    public DateTime ApplicationDate {get; set;}
    public DateTime? ApprovalDate {get; set;}
    public string Status {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public int IdOriginatingPointSale {get; set;}
    public int IdDestinationPointSale {get; set;}
    public  int IdRequestingUser {get; set;}
    public int? IdApprovingUser {get; set;}

    public PointSale? OriginatingPointSale {get; set;}
    public PointSale? DestinationPointSale {get; set;}
    public User? RequestingUser {get; set;}
    public User? ApprovingUser {get; set;}

    // Una transferencia puede tener muchos detalles
    public ICollection<InventoryTransferDetail> InventoryTransferDetails {get; set;} = new List<InventoryTransferDetail>();
}