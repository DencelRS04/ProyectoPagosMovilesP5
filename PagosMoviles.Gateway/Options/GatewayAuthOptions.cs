namespace PagosMoviles.Gateway.Options
{
    public sealed class GatewayAuthOptions
    {
        public const string SectionName = "GatewayAuth";

        public string ValidationServiceBaseUrl { get; set; } = "https://localhost:7143";
        public string ValidationEndpoint { get; set; } = "/validate";
        public int TimeoutSeconds { get; set; } = 10;
    }
}