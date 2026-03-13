
namespace PagosMoviles.PortalWeb.Models.Session
{
    public class UsuarioSesionViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
    }
}
