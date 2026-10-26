namespace TheFarm.API.Domain.Entities;

public class Sale
{
    public int IdSale {get; set;}
    public DateTime Date {get; set;}
    public decimal Total {get; set;}
    public int IdUser {get; set;}
    public int IdPointSale {get; set;}

    public User? User {get; set;}
    public PointSale? PointSale {get; set;}
    public Bill? Bill {get; set;}
    public ICollection<SaleDetail> SaleDetails {get; set;} = new List<SaleDetail>();
}