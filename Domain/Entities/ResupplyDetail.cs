namespace TheFarm.API.Domain.Entities;

public class ResupplyDetail
{
    public int IdResupplyDetail {get; set;}
    public int Quantity {get; set;}
    public int IdResupply {get; set;}
    public int IdProduct {get; set;}

    public Resupply? Resupply {get; set;}
    public Product? Product {get; set;}
}