namespace PagosMoviles.PagosMovilesService.Dtos;

public class MovimientoDto
{
    public string TipoMovimiento { get; set; } = "";
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
}