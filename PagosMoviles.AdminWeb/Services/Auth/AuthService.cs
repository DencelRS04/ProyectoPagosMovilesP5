using PagosMoviles.AdminWeb.Controllers.Auth;
using PagosMoviles.Shared.Constants;
using PagosMoviles.Shared.DTOs.Auth;
using PagosMoviles.Shared.Models;
using System.Net;
using System.Text.Json;

namespace PagosMoviles.AdminWeb.Services.Auth
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

        public async Task<(bool Exito, string Mensaje, UsuarioSesionModel? Usuario)> LoginAsync(string usuario, string contrasena)
        {
            var baseUrl = _configuration["ApiSettings:AuthBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                return (false, "No se configuró la URL del servicio de autenticación.", null);

            var endpoint = $"{baseUrl.TrimEnd('/')}/{ApiRoutes.Login}";

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

            // Según SRV5, login recibe usuario y contraseña por headers
            request.Headers.Add("usuario", usuario);
            request.Headers.Add("contrasena", contrasena);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return (false, "Usuario y/o contraseña incorrectos.", null);

            if (!response.IsSuccessStatusCode)
                return (false, "No fue posible autenticar al usuario.", null);

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(json, options);
            if (loginResponse is null)
                return (false, "La respuesta del servicio de login es inválida.", null);

            var usuarioSesion = new UsuarioSesionModel
            {
                UsuarioId = loginResponse.UsuarioID,
                UsuarioNombre = loginResponse.NombreCompleto ?? usuario,
                Rol = loginResponse.Rol ?? Roles.Admin,
                AccessToken = loginResponse.Access_Token,
                RefreshToken = loginResponse.Refresh_Token,
                ExpiraEn = loginResponse.Expires_In
            };

            return (true, string.Empty, usuarioSesion);
        }
    }
}
