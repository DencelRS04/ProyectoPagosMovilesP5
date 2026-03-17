using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PagosMoviles.API.Data;
using PagosMoviles.API.Filters;
using PagosMoviles.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers + Filtro global
builder.Services.AddControllers();
// DbContext (EF)
builder.Services.AddDbContext<PagosMovilesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servicios internos

builder.Services.AddScoped<BitacoraService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<JwtService>();

// Transacciones (SRV7, SRV8, SRV12)
builder.Services.AddScoped<ITransactionLogic, TransactionLogic>();

// Repo Dapper (SRV10/11/13/17)
builder.Services.AddScoped<PagosMovilesRepository>();

// "Puente" ConnectionStrings para Dapper Repo
// Repo usa: PagosMovilesDb y CoreBancarioDb
// appsettings tiene: DefaultConnection y CoreConnection
builder.Configuration["ConnectionStrings:PagosMovilesDb"] ??=
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Configuration["ConnectionStrings:CoreBancarioDb"] ??=
    builder.Configuration.GetConnectionString("CoreConnection");

// HttpClients
builder.Services.AddHttpClient();

builder.Services.AddHttpClient<CoreSrvClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var baseUrlRaw = cfg["Services:CoreSrv"];

    if (string.IsNullOrWhiteSpace(baseUrlRaw))
        throw new InvalidOperationException("Falta configurar Services:CoreSrv en appsettings.json");

    var baseUrl = baseUrlRaw.Trim();

    if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
        uri.Port <= 0 || uri.Port > 65535)
    {
        throw new InvalidOperationException(
            $"Services:CoreSrv inválido: '{baseUrlRaw}'. Debe ser un URL absoluto tipo 'https://localhost:7200' con puerto válido."
        );
    }

    client.BaseAddress = uri;
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PagosMoviles.API", Version = "v1" });

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

var app = builder.Build();

// Developer Exception Page + middleware de error JSON (solo dev)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var msg = app.Environment.IsDevelopment() ? ex.Message : "Error interno del servidor";

        await context.Response.WriteAsJsonAsync(new
        {
            codigo = 500,
            descripcion = msg,
            datos = (object?)null
        });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();