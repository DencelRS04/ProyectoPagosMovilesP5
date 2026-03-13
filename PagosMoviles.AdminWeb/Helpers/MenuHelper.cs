using PagosMoviles.AdminWeb.Models.Shared;
namespace PagosMoviles.AdminWeb.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItemViewModel> ObtenerMenuAdmin()
        {
            return
            [
                new() { Texto = "Inicio", Controlador = "Home", Accion = "Index" },
            new() { Texto = "Usuarios", Controlador = "Usuarios", Accion = "Index" },
            new() { Texto = "Pantallas", Controlador = "Pantallas", Accion = "Index" },
            new() { Texto = "Roles", Controlador = "Roles", Accion = "Index" },
            new() { Texto = "Parámetros", Controlador = "Parametros", Accion = "Index" },
            new() { Texto = "Entidades", Controlador = "Entidades", Accion = "Index" },
            new() { Texto = "Clientes Core", Controlador = "ClientesCore", Accion = "Index" },
            new() { Texto = "Cuentas Core", Controlador = "CuentasCore", Accion = "Index" },
            new() { Texto = "Reportes", Controlador = "Reportes", Accion = "Index" }
            ];
        }
    }
}
