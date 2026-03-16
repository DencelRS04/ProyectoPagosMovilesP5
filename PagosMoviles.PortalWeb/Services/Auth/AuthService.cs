using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PagosMoviles.Shared.Constants;
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

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "No se configuró la URL del servicio de autenticación.",
                        null
                    );
                }

                var endpoint = baseUrl.TrimEnd('/') + "/auth/login";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("usuario", usuario);
                request.Headers.Add("password", contrasena);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "Usuario y/o contraseña incorrectos.",
                        null
                    );
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "Error al autenticar. Código HTTP: " + ((int)response.StatusCode) + ". " + errorContent,
                        null
                    );
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(json, options);

                if (loginResponse == null)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "La respuesta del servicio de login es inválida.",
                        null
                    );
                }

                var usuarioSesion = new UsuarioSesionModel
                {
                    UsuarioId = loginResponse.UsuarioID.ToString(),
                    UsuarioNombre = string.IsNullOrWhiteSpace(loginResponse.NombreCompleto) ? usuario : loginResponse.NombreCompleto,
                    Rol = string.IsNullOrWhiteSpace(loginResponse.Rol) ? Roles.Admin : loginResponse.Rol,
                    AccessToken = loginResponse.Access_Token,
                    RefreshToken = loginResponse.Refresh_Token,
                    ExpiraEn = loginResponse.Expires_In
                };

                return new Tuple<bool, string, UsuarioSesionModel>(
                    true,
                    string.Empty,
                    usuarioSesion
                );
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string, UsuarioSesionModel>(
                    false,
                    "Error de conexión con el servicio de autenticación: " + ex.Message,
                    null
                );
            }
        }
    }
}