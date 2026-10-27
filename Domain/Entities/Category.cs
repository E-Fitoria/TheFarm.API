namespace TheFarm.API.Domain.Entities;

public class Category
{
    public int IdCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
    
    // 1 categoria puede tener muchos productos
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
