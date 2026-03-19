using System;

namespace PagosMoviles.Shared.Models
{
    public class UsuarioSesionModel
    {
        public string UsuarioId { get; set; }
        public string UsuarioNombre { get; set; }
        public int RolId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiraEn { get; set; }
        public string FotoPerfil { get; set; } = string.Empty;
        public string ColorAvatar { get; set; } = "#4285F4";
        public bool Bloqueado { get; set; }

        public UsuarioSesionModel()
        {
            UsuarioId = string.Empty;
            UsuarioNombre = string.Empty;
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
        }
    }
}