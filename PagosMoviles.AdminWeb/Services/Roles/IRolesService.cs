using PagosMoviles.Shared.DTOs.Roles;

namespace PagosMoviles.AdminWeb.Services.Roles
{
    public interface IRolesService
    {
        Task<List<RolDto>> ObtenerRoles();
        Task<RolDto?> ObtenerRol(int id);        // ← nuevo
        Task CrearRol(RolCreateDto dto);
        Task ActualizarRol(int id, RolCreateDto dto); // ← nuevo
        Task EliminarRol(int id);                // ← nuevo
    }
}
