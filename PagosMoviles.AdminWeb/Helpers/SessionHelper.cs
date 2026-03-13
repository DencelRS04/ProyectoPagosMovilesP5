using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.Constants;
using PagosMoviles.Shared.Models;

namespace PagosMoviles.AdminWeb.Helpers
{
    public static class SessionHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return string.IsNullOrWhiteSpace(json)
                ? default
                : JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public static void GuardarUsuarioSesion(ISession session, UsuarioSesionModel usuario)
        {
            session.SetObject(SessionKeys.UsuarioSesion, usuario);
            session.SetString(SessionKeys.LastActivityUtc, DateTime.UtcNow.ToString("O"));
        }

        public static UsuarioSesionModel? ObtenerUsuarioSesion(ISession session)
        {
            return session.GetObject<UsuarioSesionModel>(SessionKeys.UsuarioSesion);
        }

        public static void ActualizarActividad(ISession session)
        {
            session.SetString(SessionKeys.LastActivityUtc, DateTime.UtcNow.ToString("O"));
        }

        public static DateTime? ObtenerUltimaActividad(ISession session)
        {
            var value = session.GetString(SessionKeys.LastActivityUtc);
            if (DateTime.TryParse(value, out var parsed))
                return parsed;

            return null;
        }

        public static void LimpiarSesion(ISession session)
        {
            session.Remove(SessionKeys.UsuarioSesion);
            session.Remove(SessionKeys.LastActivityUtc);
        }
    }
}
