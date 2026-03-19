using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PagosMoviles.Shared.DTOs.Auth;
using PagosMoviles.Shared.Models;

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


        public async Task<Tuple<bool, string, UsuarioSesionModel>> LoginAsync(string usuario, string contrasena)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "gateway/auth/login");

                // 🔥 LIMPIO Y SIN DUPLICADOS
                request.Headers.Remove("usuario");
                request.Headers.Remove("password");

                request.Headers.Add("usuario", usuario);
                request.Headers.Add("password", contrasena);

                var response = await _httpClient.SendAsync(request);

                var json = await response.Content.ReadAsStringAsync();

                // 🔴 ERROR DESDE API
                if (!response.IsSuccessStatusCode)
                {
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        json,
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
                    return new Tuple<bool, string, UsuarioSesionModel>(
                        false,
                        "Respuesta inválida del servidor.",
                        null
                    );
                }

                var usuarioSesion = new UsuarioSesionModel
                {
                    UsuarioId = loginResponse.UsuarioID.ToString(),
                    UsuarioNombre = loginResponse.NombreCompleto,
                    RolId = loginResponse.RolId,
                    AccessToken = loginResponse.Access_Token,
                    RefreshToken = loginResponse.Refresh_Token,
                    ExpiraEn = loginResponse.Expires_In,
                    FotoPerfil = loginResponse.FotoPerfil ?? "",
                    ColorAvatar = string.IsNullOrWhiteSpace(loginResponse.ColorAvatar)
                        ? "#4285F4"
                        : loginResponse.ColorAvatar
                };

                return new Tuple<bool, string, UsuarioSesionModel>(
                    true,
                    "",
                    usuarioSesion
                );
            }
            catch (Exception ex)
            {
                return new Tuple<bool, string, UsuarioSesionModel>(
                    false,
                    ex.Message,
                    null
                );
            }
        }
    
    }
}