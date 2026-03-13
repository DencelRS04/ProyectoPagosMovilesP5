using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PagosMoviles.PagosMovilesService.Services;

namespace PagosMoviles.PagosMovilesService.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SrvAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedObjectResult(new { codigo = -1, descripcion = "Token requerido" });
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var validator = context.HttpContext.RequestServices.GetRequiredService<AuthValidationService>();

        if (!await validator.ValidateAsync(token))
        {
            context.Result = new UnauthorizedObjectResult(new { codigo = -1, descripcion = "Token inválido" });
            return;
        }

        await next();
    }
}