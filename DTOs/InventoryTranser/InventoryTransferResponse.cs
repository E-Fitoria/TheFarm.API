namespace TheFarm.API.DTOs.InventoryTransfer;

public class InventoryTransferResponse
{
    public int IdInventoryTransfer { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int IdOriginatingPointSale { get; set; }
    public string? OriginatingPointSaleName { get; set; }
    public int IdDestinationPointSale { get; set; }
    public string? DestinationPointSaleName { get; set; }

    public int IdRequestingUser { get; set; }
    public string? RequestingUsername { get; set; }
    public int? IdApprovingUser { get; set; }
    public string? ApprovingUsername { get; set; }

    public List<InventoryTransferDetailDto> Details { get; set; } = new();
}