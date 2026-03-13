using System;

namespace PagosMoviles.Shared.DTOs.Auth
{
    public class LoginResponseDto
    {
        public DateTime Expires_In { get; set; }
        public string Access_Token { get; set; } = string.Empty;
        public string Refresh_Token { get; set; } = string.Empty;
        public string UsuarioID { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}