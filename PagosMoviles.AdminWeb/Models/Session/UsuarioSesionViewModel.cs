using System;

namespace PagosMoviles.AdminWeb.Models.Session
{
    public class UsuarioSesionViewModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public string? FotoPerfil { get; set; }
        public string ColorAvatar { get; set; } = "#4285F4";
    }
}