using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.Security;
using PagosMoviles.UsuariosService.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Controladores y Filtro de Seguridad Global
builder.Services.AddControllers(o =>
{
    // Ten en cuenta que este filtro obligará a enviar un Token. 
    // Si quieres probar sin token, deberás agregar [AllowAnonymous] en tu Controller.
    o.Filters.AddService<GatewayBearerGuardFilter>();
});

// 2. Configuración de CORS (Permitir conexiones de cualquier origen, como tu celular)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PagosMoviles.UsuarioService",
        Version = "v1"
    });

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

// 3. Base de Datos
var cn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(cn))
    throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(cn));

// 4. Clientes HTTP (Gateway y Core)
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

builder.Services.AddHttpClient<CoreClientService>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrl = cfg["CoreApi:BaseUrl"];

    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta CoreApi:BaseUrl en appsettings.json");

    client.BaseAddress = new Uri(baseUrl.Trim().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// 5. Inyección de Dependencias (Services y Security)
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AfiliacionService>();
builder.Services.AddScoped<BitacoraClient>();
builder.Services.AddScoped<GatewayTokenProbe>();
builder.Services.AddScoped<GatewayBearerGuardFilter>();

var app = builder.Build();

// 6. Pipeline de la aplicación (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// IMPORTANTE: Comentamos esto para evitar errores de certificado en el celular
// app.UseHttpsRedirection(); 

app.UseStaticFiles();

// Habilitar CORS antes de mapear controladores
app.UseCors("AllowAll");

app.MapControllers();

app.Run();