namespace PagosMoviles.API.Models;

public class Pantalla
{
    public int PantallaId { get; set; }
    public string Identificador { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Ruta { get; set; } = string.Empty;
}
