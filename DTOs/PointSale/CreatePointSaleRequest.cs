namespace TheFarm.API.DTOs.PointSale;

public class CreatePointSaleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int IdUser { get; set; }
    // el statu se pone por defecto en true por el comportamiento de la tabla
}