using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.Security;
using PagosMoviles.CoreBancarioService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + filtro global de token remoto
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<CoreGatewayBearerGuardFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PagosMoviles.CoreBancarioService",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer {tu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// DB
var cn = builder.Configuration.GetConnectionString("CoreConnection");
if (string.IsNullOrWhiteSpace(cn))
    throw new InvalidOperationException("Falta ConnectionStrings:CoreConnection en appsettings.json");

builder.Services.AddDbContext<CoreDbContext>(options =>
    options.UseSqlServer(cn));

// HttpClient hacia PagosMoviles.API (validate + bitacora)
builder.Services.AddHttpClient("ApiGateway", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["GatewayApi:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta GatewayApi:BaseUrl en appsettings.json");

    client.BaseAddress = new Uri(baseUrl.Trim().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Servicios propios
builder.Services.AddScoped<CoreCuentaService>();
builder.Services.AddScoped<CoreTransaccionService>();

// Seguridad remota
builder.Services.AddScoped<CoreGatewayTokenProbe>();
builder.Services.AddScoped<CoreGatewayBearerGuardFilter>();

// Bitácora remota
builder.Services.AddScoped<CoreGatewayBitacoraClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();