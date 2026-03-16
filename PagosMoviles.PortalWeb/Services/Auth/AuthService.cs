using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PagosMoviles.Shared.DTOs.Auth;
using PagosMoviles.Shared.Models;

namespace PagosMoviles.PortalWeb.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AuthService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<Tuple<bool, string, UsuarioSesionModel>> LoginAsync(string usuario, string contrasena)
        {
            try
            {
                var baseUrl = _configuration["ApiSettings:AuthBaseUrl"];
                var endpoint = baseUrl.TrimEnd('/') + "/auth/login";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("usuario", usuario);
                request.Headers.Add("password", contrasena);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(false, "Credenciales incorrectas.", null);
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(json, options);

                if (loginResponse == null)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(false, "Login inválido.", null);
                }

                var usuarioSesion = new UsuarioSesionModel
                {
                    UsuarioId = loginResponse.UsuarioID.ToString(),
                    UsuarioNombre = loginResponse.NombreCompleto,
                    RolId = loginResponse.RolId,
                    AccessToken = loginResponse.Access_Token,
                    RefreshToken = loginResponse.Refresh_Token,
                    ExpiraEn = loginResponse.Expires_In
                };

                return new Tuple<bool, string, UsuarioSesionModel>(true, "", usuarioSesion);
            }
            catch
            {
                return new Tuple<bool, string, UsuarioSesionModel>(false, "Error de autenticación.", null);
            }
        }
    }
}