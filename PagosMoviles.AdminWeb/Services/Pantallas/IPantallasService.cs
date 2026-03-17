using PagosMoviles.Shared.DTOs.Pantallas;

namespace PagosMoviles.AdminWeb.Services.Pantallas
{
    public interface IPantallasService
    {
        Task<List<PantallaDto>> ObtenerPantallas();
        Task<PantallaDto?> ObtenerPantalla(int id);       // ← agrega si no está
        Task CrearPantalla(PantallaCreateDto dto);
        Task ActualizarPantalla(int id, PantallaCreateDto dto); // ← agrega el int id
        Task EliminarPantalla(int id);                    // ← agrega si no está
    }
}