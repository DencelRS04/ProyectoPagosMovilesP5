namespace PagosMoviles.Shared.DTOs.Usuarios
{
    public class UsuarioPerfilDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FotoPerfil { get; set; } = string.Empty;
        public string ColorAvatar { get; set; } = "#4285F4";
    }
}