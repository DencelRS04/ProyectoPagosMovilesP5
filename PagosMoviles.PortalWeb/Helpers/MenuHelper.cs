using PagosMoviles.PortalWeb.Models.Shared;
namespace PagosMoviles.PortalWeb.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItemViewModel> ObtenerMenuPortal()
        {
            return
            [
                new() { Texto = "Inicio", Controlador = "Home", Accion = "Index" },
            new() { Texto = "Inscripción", Controlador = "Afiliacion", Accion = "Index" },
            new() { Texto = "Saldo", Controlador = "Saldo", Accion = "Index" },
            new() { Texto = "Movimientos", Controlador = "Movimientos", Accion = "Index" },
            new() { Texto = "Transferencias", Controlador = "Transferencias", Accion = "Index" }
            ];
        }
    }
}
