namespace TheFarm.API.DTOs.PointSale;

// informacion que se muestra en el frontend del punto de venta
public class PointSaleResponse
{
    public int IdPointSale { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool Status { get; set; }

    public int IdUser { get; set; }

    //User manager
    public string? ManagerUserName { get; set; }
    public string? ManagerName { get; set; }
}