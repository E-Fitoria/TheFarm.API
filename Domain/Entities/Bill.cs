namespace TheFarm.API.Domain.Entities;

public class Bill
{
    public int IdBill {get; set;}
    public int IdClient {get; set;}
    public int IdSale {get; set;}
    public string EmailStatus {get; set;} = string.Empty;
    public DateTime? EmailSendAt {get; set;}

    public Client? Client {get; set;}
    public Sale? Sale {get; set;}
}