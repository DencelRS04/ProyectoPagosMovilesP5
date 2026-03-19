using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PagosMoviles.Shared.DTOs.Auth;
using PagosMoviles.Shared.Models;
using PagosMoviles.AdminWeb.Models.Usuarios;
using Microsoft.AspNetCore.Http;

namespace PagosMoviles.PortalWeb.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        // 🔵 LOGIN
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

                if (!response.IsSuccessStatusCode)
                {
                    var texto = json.ToLower();

                    if (texto.Contains("bloqueado"))
                        return new Tuple<bool, string, UsuarioSesionModel>(false, "El usuario se encuentra bloqueado.", null);

                    if (texto.Contains("intentos"))
                        return new Tuple<bool, string, UsuarioSesionModel>(false, "El usuario se bloqueó por fallar 3 veces.", null);

                    return new Tuple<bool, string, UsuarioSesionModel>(false, "Usuario y/o contraseña incorrectos.", null);
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(json, options);

                if (loginResponse == null)
                    return new Tuple<bool, string, UsuarioSesionModel>(false, "Login inválido.", null);

                var usuarioSesion = new UsuarioSesionModel
                {
                    UsuarioId = loginResponse.UsuarioID.ToString(),
                    UsuarioNombre = loginResponse.NombreCompleto,
                    RolId = loginResponse.RolId,
                    AccessToken = loginResponse.Access_Token,
                    RefreshToken = loginResponse.Refresh_Token,
                    ExpiraEn = loginResponse.Expires_In
                };

                // 🔥 RESET TOTAL DE INTENTOS
                var session = _httpContextAccessor.HttpContext?.Session;

                if (session != null)
                {
                    var keys = session.Keys;

                    foreach (var key in keys)
                    {
                        if (key.StartsWith("Intentos_"))
                        {
                            session.Remove(key);
                        }
                    }

                    session.Remove("UsuarioIntento");
                }

                return new Tuple<bool, string, UsuarioSesionModel>(true, "", usuarioSesion);
            }
            catch
            {
                return new Tuple<bool, string, UsuarioSesionModel>(false, "Error de autenticación.", null);
            }
        }

        // 🔥 REGISTER
        public async Task<Tuple<bool, string>> RegisterAsync(UsuarioFormModel model)
        {
            try
            {
                var baseUrl = _configuration["GatewayApi:BaseUrl"];
                var endpoint = baseUrl.TrimEnd('/') + "/user";

                var jsonContent = JsonSerializer.Serialize(new
                {
                    NombreCompleto = model.NombreCompleto,
                    Email = model.Email,
                    Identificacion = model.Identificacion,
                    Telefono = model.Telefono,
                    Password = model.Password,
                    RolId = 2,
                    TipoIdentificacion = model.TipoIdentificacion ?? "CC"
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Tuple<bool, string>(false, responseText);
                }

                return new Tuple<bool, string>(true, "Usuario registrado correctamente.");
            }
            catch
            {
                return new Tuple<bool, string>(false, "Error de conexión con el servidor.");
            }
        }
    }
}