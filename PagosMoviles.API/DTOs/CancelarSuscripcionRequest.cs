namespace PagosMoviles.PagosMovilesService.Dtos;

public class CancelarSuscripcionRequest
{
    public string NumeroCuenta { get; set; } = "";
    public string Identificacion { get; set; } = "";
    public string Telefono { get; set; } = "";
}