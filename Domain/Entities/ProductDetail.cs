namespace TheFarm.API.Domain.Entities;

public class ProductDetail
{
    public int IdProductDetail {get; set;}
    public DateTime PurchaseDate {get; set;}
    public int Quantity {get; set;}
    public decimal PurchasePrice {get; set;}
    public int IdProduct {get; set;}
    public int IdPointSale {get; set;}

    // propiedad de navegacion para la relacion con la tabla Product
    public Product? Product {get; set;}
    // propiedad de navegacion para la relacion con la tabla PointSale
    public PointSale? PointSale {get; set;}
}