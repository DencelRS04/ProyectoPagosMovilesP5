using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using PagosMoviles.AdminWeb.Models.Shared;
using PagosMoviles.Shared.Constants;

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

            if (usuario.RolId == Roles.Admin)
            {
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Home/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Usuarios/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Pantallas/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Roles/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Parametros/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Entidades/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/ClientesCore/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/CuentasCore/Index" });
                menu.Add(new MenuItemViewModel { Texto = "", Url = "/Reportes/Index" });
            }

            return menu;
        }
    }
}