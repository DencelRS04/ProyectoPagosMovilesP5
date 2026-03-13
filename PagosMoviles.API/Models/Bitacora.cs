namespace PagosMoviles.API.Models;

public class Bitacora
{
    public int BitacoraId { get; set; }
    public DateTime Fecha { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty; 
}
