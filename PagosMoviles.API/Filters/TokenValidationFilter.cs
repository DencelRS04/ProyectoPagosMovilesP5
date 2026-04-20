using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.API.Services;

namespace PagosMoviles.API.Filters
{
    public class TokenValidationFilter : IAsyncActionFilter
    {
        private readonly TokenService _tokenService;

        public TokenValidationFilter(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Si el endpoint tiene [AllowAnonymous], no validamos token
            var endpoint = context.HttpContext.GetEndpoint();
            var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

            if (allowAnonymous)
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
                    descripcion = "Token requerido"
                });
                return;
            }

            var token = auth["Bearer ".Length..].Trim();

            var ok = await _tokenService.ValidarJwtAsync(token);

            if (!ok)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    codigo = 401,
                    descripcion = "Token inválido o expirado"
                });
                return;
            }

            await next();
        }
    }
}