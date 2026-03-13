namespace PagosMoviles.API.DTOs;

public class PagoMovilMiniDto
{
    public string Identificacion { get; set; } = "";
    public string NumeroCuenta { get; set; } = "";
    public string Telefono { get; set; } = "";
    public bool Estado { get; set; }
}