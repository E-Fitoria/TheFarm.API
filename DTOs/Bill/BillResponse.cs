namespace TheFarm.API.DTOs.Bill;

public class BillResponse
{
    public int IdBill { get; set; }
    public int IdClient { get; set; }
    public string? ClientName { get; set; }
    public string? ClientEmail { get; set; }
    public int IdSale { get; set; }
    public decimal? SaleTotal { get; set; }
    public DateTime? SaleDate { get; set; }
    public string EmailStatus { get; set; } = string.Empty;
    public DateTime? EmailSendAt { get; set; }
}
