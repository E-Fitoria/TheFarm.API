namespace TheFarm.API.Domain.Entities;

public class Product
{
    public int IdProduct {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public bool AllowsDoseSale {get; set;}
    public int IdCategory {get; set;}
    public decimal SalePrice {get; set;}
    public decimal DoseSalePrice {get; set;}
    public int ServicesNumber {get; set;}
    public string Status {get; set;} = string.Empty;

    // propiedad de navegacion para la relacion con la tabla Category
    public Category? Category {get; set;}
}