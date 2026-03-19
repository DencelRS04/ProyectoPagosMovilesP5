using Microsoft.AspNetCore.DataProtection;
using PagosMoviles.AdminWeb.Services;
using PagosMoviles.AdminWeb.Services.Auth;
using PagosMoviles.AdminWeb.Services.Perfil;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.AdminWeb.Services.ClientesCore;
using PagosMoviles.AdminWeb.Services.Entidades;
using PagosMoviles.AdminWeb.Services.Reporte;
using PagosMoviles.AdminWeb.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("PagosMoviles.AdminWeb");

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Todos los clientes apuntan al Gateway
var gatewayBaseUrl = builder.Configuration["GatewayApi:BaseUrl"];
if (string.IsNullOrWhiteSpace(gatewayBaseUrl))
    throw new InvalidOperationException("Falta GatewayApi:BaseUrl en appsettings.json");

var gatewayHandler = new Func<HttpClientHandler>(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Registra el DelegatingHandler que añade el Bearer token
builder.Services.AddTransient<BearerTokenHandler>();

// Cliente principal
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri(gatewayBaseUrl.Trim().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(gatewayHandler)
.AddHttpMessageHandler<BearerTokenHandler>();

// Alias usados por RolesService, PantallasService, Parametros y Usuarios pages
foreach (var nombre in new[] { "gateway", "ParametroApi", "UsuarioApi" })
{
    builder.Services.AddHttpClient(nombre, client =>
    {
        client.BaseAddress = new Uri(gatewayBaseUrl.Trim().TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    })
    .ConfigurePrimaryHttpMessageHandler(gatewayHandler)
    .AddHttpMessageHandler<BearerTokenHandler>();
}

builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IPerfilService, PerfilService>();

builder.Services.AddScoped<IPantallasService, PantallasService>();
builder.Services.AddScoped<IRolesService, RolesService>();
builder.Services.AddScoped<ClientesCoreService>();
builder.Services.AddScoped<EntidadService>();
builder.Services.AddScoped<ReporteService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AdminWeb.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/Home/Index");
        return Task.CompletedTask;
    });
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();        // ✅ Session antes de Authorization
app.UseAuthorization();
app.MapRazorPages();
app.Run();