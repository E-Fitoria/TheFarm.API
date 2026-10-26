namespace TheFarm.API.Domain.Entities;

public class Resupply
{
    public int IdResupply {get; set;}
    public string Status {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public int IdPointSale {get; set;}
    public int IdUser {get; set;}

    public PointSale? PointSale {get; set;}
    public User? User {get; set;}
    public ICollection<ResupplyDetail> ResupplyDetails {get; set;} = new List<ResupplyDetail>();
}