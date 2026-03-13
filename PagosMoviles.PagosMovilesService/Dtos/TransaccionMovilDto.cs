namespace PagosMoviles.PagosMovilesService.Dtos;

public class TransaccionMovilDto
{
    public int TransaccionId { get; set; }
    public string? EntidadOrigen { get; set; }
    public string? EntidadDestino { get; set; }
    public string? TelefonoOrigen { get; set; }
    public string? TelefonoDestino { get; set; }
    public decimal Monto { get; set; }
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
}