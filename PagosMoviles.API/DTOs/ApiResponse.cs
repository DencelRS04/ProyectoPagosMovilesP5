namespace PagosMoviles.API.DTOs;

public class ApiResponse
{
    public int codigo { get; set; }
    public string descripcion { get; set; } = "";
    public object? datos { get; set; }
}