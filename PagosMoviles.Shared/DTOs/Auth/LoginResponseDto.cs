using System;

namespace PagosMoviles.Shared.DTOs.Auth
{
    public class LoginResponseDto
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; }
        public DateTime Expires_In { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public int UsuarioID { get; set; }
        public string NombreCompleto { get; set; }
        public string Rol { get; set; }
        public string FotoPerfil { get; set; } = string.Empty;
        public string ColorAvatar { get; set; } = "#4285F4";

        public LoginResponseDto()
        {
            Descripcion = string.Empty;
            Access_Token = string.Empty;
            Refresh_Token = string.Empty;
            NombreCompleto = string.Empty;
            Rol = string.Empty;
        }
    }
}