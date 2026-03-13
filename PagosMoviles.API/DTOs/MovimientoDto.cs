namespace PagosMoviles.API.DTOs;

public class MovimientoDto
{
    public string TipoMovimiento { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
}