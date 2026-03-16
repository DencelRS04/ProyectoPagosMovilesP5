using Microsoft.AspNetCore.Http;

namespace PagosMoviles.UsuariosService.DTOs
{
    public class UsuarioPerfilUpdateDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string ColorAvatar { get; set; } = "#4285F4";
        public IFormFile? Imagen { get; set; }
    }
}