namespace TheFarm.API.Domain.Entities;

public class SaleDetail
{
    public int IdSaleDetail {get; set;}
    public int Quantity {get; set;}
    public decimal UnitPrice {get; set;}
    public int IdSale {get; set;}
    public int IdProduct {get; set;}
    public string SaleType {get; set;} = string.Empty;

    public Sale? Sale {get; set;}
    public Product? Product {get; set;}
}