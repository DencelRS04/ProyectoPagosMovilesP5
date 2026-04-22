namespace PagosMoviles.Gateway.Security
{
    public static class GatewayRoutePolicy
    {
        public static bool RequiresToken(PathString path)
        {
            var value = path.Value?.ToLowerInvariant() ?? string.Empty;

            // Solo protegemos tráfico que entra por el gateway
            if (!value.StartsWith("/gateway"))
                return false;

            // Swagger / herramientas
            if (value.StartsWith("/swagger"))
                return false;

            // Login y refresh no se protegen en GTW2
            if (value == "/gateway/auth/login")
                return false;

            if (value == "/gateway/auth/refresh")
                return false;

            // La validación misma no debe pedir token o entrarías en bucle
            if (value == "/gateway/auth/validate")
                return false;

            // SRV7 no requiere token
            // Soportamos ambas variantes para no romper tus rutas actuales.
            if (value == "/gateway/admin/transactions/process")
                return false;

            if (value == "/gateway/trans/transactions/process")
                return false;

            return true;
        }
    }
}