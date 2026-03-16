using Microsoft.AspNetCore.Http;

namespace PagosMoviles.PortalWeb.Models.Perfil
{
    public class PerfilViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Identificacion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string FotoPerfil { get; set; }
        public string ColorAvatar { get; set; }
        public IFormFile? Imagen { get; set; }

        public PerfilViewModel()
        {
            NombreCompleto = string.Empty;
            Identificacion = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            FotoPerfil = string.Empty;
            ColorAvatar = "#4285F4";
        }
    }
}