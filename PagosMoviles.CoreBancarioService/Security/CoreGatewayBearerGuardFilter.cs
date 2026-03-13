using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PagosMoviles.CoreBancarioService.Security
{
    public sealed class CoreGatewayBearerGuardFilter : IAsyncActionFilter
    {
        private readonly CoreGatewayTokenProbe _probe;

        public CoreGatewayBearerGuardFilter(CoreGatewayTokenProbe probe)
        {
            _probe = probe;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var endpoint = context.HttpContext.GetEndpoint();

            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await next();
                return;
            }

            var auth = context.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(auth) ||
                !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    codigo = 401,
                    descripcion = "Token requerido",
                    datos = (object?)null
                });
                return;
            }

            var token = auth.Substring("Bearer ".Length).Trim();

            var ok = await _probe.IsTokenValidAsync(token);

            if (!ok)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    codigo = 401,
                    descripcion = "Token inválido o expirado",
                    datos = (object?)null
                });
                return;
            }

            await next();
        }
    }
}