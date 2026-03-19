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
                var baseUrl = _configuration["GatewayApi:BaseUrl"];
                var endpoint = baseUrl.TrimEnd('/') + "/gateway/auth/login";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
                request.Headers.Add("usuario", usuario);
                request.Headers.Add("password", contrasena);

                var response = await _httpClient.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();

                // 🔴 SI FALLA EL LOGIN
                if (!response.IsSuccessStatusCode)
                {
                    var texto = json.ToLower();

                    // Usuario bloqueado
                    if (texto.Contains("bloqueado"))
                    {
                        return new Tuple<bool, string, UsuarioSesionModel>(
                            false,
                            "El usuario se encuentra bloqueado.",
                            null
                        );
                    }

                    // 3 intentos fallidos
                    if (texto.Contains("intentos") || texto.Contains("3"))
                    {
                        return new Tuple<bool, string, UsuarioSesionModel>(
                            false,
                            "El usuario se bloqueó por fallar 3 veces.",
                            null
                        );
                    }

                    // Credenciales incorrectas
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "Usuario y/o contraseña incorrectos.",
                        null
                    );
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(json, options);

                if (loginResponse == null)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(false, "Login inválido.", null);
                }

                // 🔵 NO SE TOCA EL LOGIN EXITOSO
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