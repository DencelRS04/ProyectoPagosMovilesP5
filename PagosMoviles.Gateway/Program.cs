using PagosMoviles.Gateway.Middleware;
using PagosMoviles.Gateway.Options;
using PagosMoviles.Gateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Configuration/yarp.json", optional: false, reloadOnChange: true);

// Configuración de GTW2
builder.Services.Configure<GatewayAuthOptions>(
    builder.Configuration.GetSection(GatewayAuthOptions.SectionName));

// Cliente que llama al servicio de validación del token
builder.Services.AddHttpClient<ITokenValidationClient, TokenValidationClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

// YARP
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Servicios base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Middleware de GTW2 antes del proxy
app.UseMiddleware<GatewayAuthMiddleware>();

app.MapControllers();
app.MapReverseProxy();

app.Run();