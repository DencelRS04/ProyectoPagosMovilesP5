using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PagosMoviles.AdminWeb.Helpers;
using PagosMoviles.AdminWeb.Models.Perfil;

namespace PagosMoviles.AdminWeb.Services.Perfil
{
    public class PerfilService : IPerfilService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PerfilService(
            HttpClient httpClient,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PerfilResponseDto> ObtenerPerfilAsync(int usuarioId)
        {
            try
            {
                var baseUrl = _configuration["GatewayApi:BaseUrl"];
                var endpoint = baseUrl!.TrimEnd('/') + "/user/" + usuarioId;

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);

                var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(_httpContextAccessor.HttpContext!.Session);
                if (usuarioSesion != null && !string.IsNullOrWhiteSpace(usuarioSesion.AccessToken))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", usuarioSesion.AccessToken);

                var response = await _httpClient.SendAsync(request);
                var contenido = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return new PerfilResponseDto
                    {
                        Exito = false,
                        ErrorDetalle = $"HTTP {(int)response.StatusCode}: {contenido}"
                    };

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var wrapper = JsonSerializer.Deserialize<UsuarioDetalleWrapper>(contenido, options);

                if (wrapper?.Datos == null)
                    return new PerfilResponseDto
                    {
                        Exito = false,
                        ErrorDetalle = "La respuesta del servicio llegó vacía o inválida."
                    };

                return new PerfilResponseDto
                {
                    Exito = true,
                    UsuarioId = wrapper.Datos.UsuarioId,
                    NombreCompleto = wrapper.Datos.NombreCompleto ?? string.Empty,
                    Identificacion = wrapper.Datos.Identificacion ?? string.Empty,
                    Telefono = wrapper.Datos.Telefono ?? string.Empty,
                    Email = wrapper.Datos.Email ?? string.Empty,
                    FotoPerfil = ConstruirUrlFoto(wrapper.Datos.FotoPerfil),
                    ColorAvatar = string.IsNullOrWhiteSpace(wrapper.Datos.ColorAvatar)
                        ? "#4285F4"
                        : wrapper.Datos.ColorAvatar
                };
            }
            catch (Exception ex)
            {
                return new PerfilResponseDto { Exito = false, ErrorDetalle = ex.Message };
            }
        }

        public async Task<bool> ActualizarPerfilAsync(PerfilViewModel model)
        {
            try
            {
                var baseUrl = _configuration["GatewayApi:BaseUrl"];
                var endpoint = baseUrl!.TrimEnd('/') + "/user/actualizar-perfil";

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(model.UsuarioId.ToString()), "UsuarioId");
                content.Add(new StringContent(model.NombreCompleto ?? string.Empty), "NombreCompleto");
                content.Add(new StringContent(model.Telefono ?? string.Empty), "Telefono");
                content.Add(new StringContent(model.ColorAvatar ?? "#4285F4"), "ColorAvatar");

                if (model.Imagen != null && model.Imagen.Length > 0)
                {
                    var streamContent = new StreamContent(model.Imagen.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(model.Imagen.ContentType);
                    content.Add(streamContent, "Imagen", model.Imagen.FileName);
                }

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint) { Content = content };

                var usuarioSesion = SessionHelper.ObtenerUsuarioSesion(_httpContextAccessor.HttpContext!.Session);
                if (usuarioSesion != null && !string.IsNullOrWhiteSpace(usuarioSesion.AccessToken))
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", usuarioSesion.AccessToken);

                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // Las fotos se sirven directamente desde UsuariosService (no pasan por Gateway)
        private string ConstruirUrlFoto(string rutaFoto)
        {
            if (string.IsNullOrWhiteSpace(rutaFoto))
                return string.Empty;

            if (rutaFoto.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                rutaFoto.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return rutaFoto;

            var baseUrl = _configuration["ApiSettings:UsuarioBaseUrl"] ?? string.Empty;
            return baseUrl.TrimEnd('/') + "/" + rutaFoto.TrimStart('/');
        }
    }

    public class UsuarioDetalleWrapper
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public UsuarioDetalleDto Datos { get; set; } = new();
    }

    public class UsuarioDetalleDto
    {
        public int UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public string FotoPerfil { get; set; } = string.Empty;
        public string ColorAvatar { get; set; } = "#4285F4";
    }
}