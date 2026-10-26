namespace TheFarm.API.Domain.Entities;

public class Inventory
{
    public int IdInventory {get; set;}
    public int IdPointSale {get; set;}
    public int IdProduct {get; set;}
    public int UnitsAvailable {get; set;}
    public int MinStock {get; set;}
    public int OpenUnitRemainingDoses {get; set;} // dosis que sobran de una unidad abierta

    public PointSale? PointSale {get; set;}
    public Product? Product {get; set;}
}