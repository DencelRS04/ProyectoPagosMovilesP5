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
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session != null)
            {
                var json = session.GetString(SessionKeys.UsuarioSesion);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var usuario = JsonSerializer.Deserialize<UsuarioSesionModel>(json);
                    if (!string.IsNullOrWhiteSpace(usuario?.AccessToken))
                    {
                        request.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", usuario.AccessToken);
                    }
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}