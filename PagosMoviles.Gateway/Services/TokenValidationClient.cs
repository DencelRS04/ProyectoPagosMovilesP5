using System.Text.Json;
using Microsoft.Extensions.Options;
using PagosMoviles.Gateway.Options;

namespace PagosMoviles.Gateway.Services
{
    public sealed class TokenValidationClient : ITokenValidationClient
    {
        private readonly HttpClient _httpClient;
        private readonly GatewayAuthOptions _options;

        public TokenValidationClient(
            HttpClient httpClient,
            IOptions<GatewayAuthOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;

            _httpClient.BaseAddress = new Uri(_options.ValidationServiceBaseUrl.TrimEnd('/') + "/");
            _httpClient.Timeout = TimeSpan.FromSeconds(
                _options.TimeoutSeconds <= 0 ? 10 : _options.TimeoutSeconds);
        }

        public async Task<bool> ValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var endpoint = _options.ValidationEndpoint.TrimStart('/');

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return false;

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(content))
                    return false;

                content = content.Trim();

                try
                {
                    using var doc = JsonDocument.Parse(content);

                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        if (doc.RootElement.TryGetProperty("data", out var dataProp) &&
                            dataProp.ValueKind == JsonValueKind.True)
                        {
                            return true;
                        }

                        if (doc.RootElement.TryGetProperty("codigo", out var codigoProp) &&
                            codigoProp.ValueKind == JsonValueKind.Number &&
                            codigoProp.GetInt32() == 200)
                        {
                            return true;
                        }
                    }

                    if (doc.RootElement.ValueKind == JsonValueKind.True)
                        return true;
                }
                catch
                {
                    if (string.Equals(content, "true", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}