using System;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.Constants;
using PagosMoviles.Shared.Models;

namespace PagosMoviles.PortalWeb.Helpers
{
    public static class SessionHelper
    {
        public static void GuardarUsuarioSesion(ISession session, UsuarioSesionModel usuario)
        {
            var json = JsonSerializer.Serialize(usuario);
            session.SetString(SessionKeys.UsuarioSesion, json);

            // Guardar siempre en formato UTC correcto
            session.SetString(
                SessionKeys.LastActivityUtc,
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            );
        }

        public static UsuarioSesionModel ObtenerUsuarioSesion(ISession session)
        {
            var json = session.GetString(SessionKeys.UsuarioSesion);

            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return JsonSerializer.Deserialize<UsuarioSesionModel>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void ActualizarActividad(ISession session)
        {
            session.SetString(
                SessionKeys.LastActivityUtc,
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
            );
        }

        public static DateTime? ObtenerUltimaActividad(ISession session)
        {
            var valor = session.GetString(SessionKeys.LastActivityUtc);

            if (string.IsNullOrWhiteSpace(valor))
                return null;

            DateTime fecha;
            var ok = DateTime.TryParseExact(
                valor,
                "o",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out fecha
            );

            if (!ok)
                return null;

            return fecha.ToUniversalTime();
        }

        public static void LimpiarSesion(ISession session)
        {
            session.Remove(SessionKeys.UsuarioSesion);
            session.Remove(SessionKeys.LastActivityUtc);
            session.Remove(SessionKeys.SessionExpiredMessage);
        }
    }
}