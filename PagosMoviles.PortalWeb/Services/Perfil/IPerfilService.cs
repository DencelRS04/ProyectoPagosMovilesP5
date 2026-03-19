using PagosMoviles.PortalWeb.Models.Perfil;

namespace PagosMoviles.PortalWeb.Services.Perfil
{
    public interface IPerfilService
    {
        Task<PerfilResponseDto> ObtenerPerfilAsync(int usuarioId);
        Task<bool> ActualizarPerfilAsync(PerfilViewModel model);
    }

    public class PerfilResponseDto
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Identificacion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string FotoPerfil { get; set; }
        public string ColorAvatar { get; set; }
        public bool Exito { get; set; }
        public string ErrorDetalle { get; set; }

        public PerfilResponseDto()
        {
            NombreCompleto = string.Empty;
            Identificacion = string.Empty;
            Telefono = string.Empty;
            Email = string.Empty;
            FotoPerfil = string.Empty;
            ColorAvatar = "#4285F4";
            ErrorDetalle = string.Empty;
        }
    }
}