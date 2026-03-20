using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PagosMoviles.Shared.Constants;
using PagosMoviles.Shared.Models;

namespace PagosMoviles.PortalWeb.Handlers
{
    public class BearerTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BearerTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
      HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"[PORTAL HANDLER] *** EJECUTANDO ***");

            var session2 = _httpContextAccessor.HttpContext?.Session;
            Console.WriteLine($"[PORTAL HANDLER] Session null: {session2 == null}");

            var jsonSesion = session2?.GetString("USUARIO_SESION");
            Console.WriteLine($"[PORTAL HANDLER] Json vacío: {string.IsNullOrWhiteSpace(jsonSesion)}");

            if (session2 != null)
            {
                var jsonData = session2.GetString(SessionKeys.UsuarioSesion);
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    var usuario = JsonSerializer.Deserialize<UsuarioSesionModel>(jsonData);
                    Console.WriteLine($"[PORTAL HANDLER] Token: '{usuario?.AccessToken}'");

                    if (!string.IsNullOrWhiteSpace(usuario?.AccessToken))
                    {
                        request.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", usuario.AccessToken);
                        Console.WriteLine($"[PORTAL HANDLER] Token adjuntado OK");
                    }
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}