using System;
using System.Threading.Tasks;
using PagosMoviles.Shared.Models;

namespace PagosMoviles.AdminWeb.Services.Auth
{
    public interface IAuthService
    {
        Task<Tuple<bool, string, UsuarioSesionModel>> LoginAsync(string usuario, string contrasena);
    }
}