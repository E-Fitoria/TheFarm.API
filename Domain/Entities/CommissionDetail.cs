namespace TheFarm.API.Domain.Entities;

public class CommissionDetail
{
    public int IdCommissionDetail {get; set;}
    public float CommissionPercentage {get; set;}
    public float CommissionAmount {get; set;}
    public DateTime CalculatedDate {get; set;}
    public int IdCommission {get; set;}
    public int IdSale {get; set;}

    public Commission? Commission {get; set;}
    public Sale? Sale {get; set;}

    
}