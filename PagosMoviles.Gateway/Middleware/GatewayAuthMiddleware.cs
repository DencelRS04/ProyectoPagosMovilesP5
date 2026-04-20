using System.Text.Json;
using PagosMoviles.Gateway.Security;
using PagosMoviles.Gateway.Services;

namespace PagosMoviles.Gateway.Middleware
{
    public sealed class GatewayAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public GatewayAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ITokenValidationClient tokenValidationClient)
        {
            if (!GatewayRoutePolicy.RequiresToken(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var authorization = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authorization) ||
                !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                await WriteUnauthorizedAsync(context, "Token requerido.");
                return;
            }

            var token = authorization["Bearer ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(token))
            {
                await WriteUnauthorizedAsync(context, "Token inválido o expirado.");
                return;
            }

            var isValid = await tokenValidationClient.ValidateAsync(
                token,
                context.RequestAborted);

            if (!isValid)
            {
                await WriteUnauthorizedAsync(context, "Token inválido o expirado.");
                return;
            }

            // No modificamos el Authorization header.
            // Se deja pasar intacto para no romper los filtros/guards
            // que todavía existen en servicios downstream.
            await _next(context);
        }

        private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                codigo = 401,
                descripcion = message,
                datos = (object?)null
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}