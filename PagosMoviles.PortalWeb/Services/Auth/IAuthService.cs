using PagosMoviles.Shared.Models;
namespace PagosMoviles.PortalWeb.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Exito, string Mensaje, UsuarioSesionModel? Usuario)> LoginAsync(string usuario, string contrasena);
    }
}
