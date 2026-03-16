using System;

namespace PagosMoviles.AdminWeb.Models.Session
{
    public class UsuarioSesionViewModel
    {
        public string UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public int RolId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiraEn { get; set; }
    }
}