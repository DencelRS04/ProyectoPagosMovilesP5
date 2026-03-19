namespace PagosMoviles.API.DTOs;

public class PantallaDto
{
    public int PantallaId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
}
