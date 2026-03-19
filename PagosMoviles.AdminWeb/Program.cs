using Microsoft.AspNetCore.DataProtection;
using PagosMoviles.AdminWeb.Services;
using PagosMoviles.AdminWeb.Services.Auth;
using PagosMoviles.AdminWeb.Services.Perfil;
using PagosMoviles.AdminWeb.Services.Pantallas;
using PagosMoviles.AdminWeb.Services.Roles;
using PagosMoviles.AdminWeb.Services.ClientesCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
    .SetApplicationName("PagosMoviles.AdminWeb");

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// Gateway config
var gatewayBaseUrl = builder.Configuration["GatewayApi:BaseUrl"];
if (string.IsNullOrWhiteSpace(gatewayBaseUrl))
    throw new InvalidOperationException("Falta GatewayApi:BaseUrl en appsettings.json");

var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

// HttpClients
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri(gatewayBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
}).ConfigurePrimaryHttpMessageHandler(() => handler);

foreach (var nombre in new[] { "gateway", "ParametroApi", "UsuarioApi" })
{
    builder.Services.AddHttpClient(nombre, client =>
    {
        client.BaseAddress = new Uri(gatewayBaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    }).ConfigurePrimaryHttpMessageHandler(() => handler);
}

builder.Services.AddHttpClient<IAuthService, AuthService>((provider, client) =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var baseUrl = config["GatewayApi:BaseUrl"];

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => handler);

builder.Services.AddHttpClient<IPerfilService, PerfilService>();

builder.Services.AddScoped<IPantallasService, PantallasService>();
builder.Services.AddScoped<IRolesService, RolesService>();
builder.Services.AddScoped<ClientesCoreService>();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AdminWeb.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    app.MapGet("/", context =>
    {
        context.Response.Redirect("/");
        return Task.CompletedTask;
    });
}

app.UseStaticFiles();
app.UseRouting();

// 🔥 PRIMERO session
app.UseSession();

// 🔥 DESPUÉS el middleware
app.Use(async (context, next) =>
{
    var usuarioAntes = context.Session.GetString("UsuarioId");

    await next();

    var usuarioDespues = context.Session.GetString("UsuarioId");

    if (!string.IsNullOrEmpty(usuarioAntes) && string.IsNullOrEmpty(usuarioDespues))
    {
        context.Session.SetString("SessionExpiredMessage", "La sesión expiró por inactividad.");
    }
});

app.UseAuthorization();
app.MapRazorPages();

app.Run();