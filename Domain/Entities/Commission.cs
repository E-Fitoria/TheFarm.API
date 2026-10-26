namespace TheFarm.API.Domain.Entities;

public class Commission
{
    public int IdCommission {get; set;}
    public float Percentage {get; set;}
    public DateTime Date {get; set;}
    public DateTime? EndDate {get; set;}
    public int IdUser {get; set;}

    public User? User {get; set;}

    public ICollection<CommissionDetail> CommissionDetails {get; set;} = new List<CommissionDetail>();
}