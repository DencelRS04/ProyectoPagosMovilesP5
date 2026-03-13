using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.Security;
using PagosMoviles.UsuariosService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + filtro global (valida token preguntándole a la API)
builder.Services.AddControllers(o =>
{
    o.Filters.AddService<GatewayBearerGuardFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PagosMoviles.UsuarioService", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// DB
var cn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(cn))
    throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(cn));

// HttpClient hacia PagosMoviles.API
builder.Services.AddHttpClient("GatewayApi", (sp, client) =>
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

// DI (tus servicios)
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AfiliacionService>();
builder.Services.AddScoped<CoreClientService>();
builder.Services.AddScoped<BitacoraClient>();

// Seguridad nueva (nombres nuevos)
builder.Services.AddScoped<GatewayTokenProbe>();
builder.Services.AddScoped<GatewayBearerGuardFilter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();