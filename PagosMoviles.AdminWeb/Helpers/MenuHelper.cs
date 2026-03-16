using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using PagosMoviles.AdminWeb.Models.Shared;

namespace PagosMoviles.AdminWeb.Helpers
{
    public static class MenuHelper
    {
        public static List<MenuItemViewModel> ObtenerMenuAdmin(ISession session)
        {
            var usuario = SessionHelper.ObtenerUsuarioSesion(session);
            var menu = new List<MenuItemViewModel>();

            if (usuario == null)
                return menu;

            if (usuario.Rol == "ADMIN")
            {
                menu.Add(new MenuItemViewModel { Texto = "Bienvenida", Url = "/Home/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Usuarios", Url = "/Usuarios/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Pantallas", Url = "/Pantallas/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Roles", Url = "/Roles/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Parámetros", Url = "/Parametros/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Entidades", Url = "/Entidades/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Clientes Core", Url = "/ClientesCore/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Cuentas Core", Url = "/CuentasCore/Index" });
                menu.Add(new MenuItemViewModel { Texto = "Reportes", Url = "/Reportes/Index" });
            }

            return menu;
        }
    }
}