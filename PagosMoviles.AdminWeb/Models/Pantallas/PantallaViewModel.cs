namespace PagosMoviles.AdminWeb.Models.Pantallas
{
    public class PantallaViewModel
    {
        public int PantallaId { get; set; }
        public string Identificador { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Ruta { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string? MensajeError { get; set; }
    }
}
