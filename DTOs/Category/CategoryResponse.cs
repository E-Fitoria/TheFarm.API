namespace TheFarm.API.DTOs.Category;

public class CategoryResponse
{
    public int IdCategory { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Status { get; set; }
}

