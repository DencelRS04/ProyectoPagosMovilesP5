using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.PortalWeb.Services.Perfil;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.PortalWeb.Services.Transferencias;
using PagosMoviles.PortalWeb.Services.Afiliacion;
using PagosMoviles.PortalWeb.Handlers;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Razor
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// 🔥 HTTP CLIENT AL GATEWAY
builder.Services.AddHttpClient("GatewayApi", client =>
{
    var baseUrl = builder.Configuration["GatewayApi:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("Falta GatewayApi:BaseUrl en appsettings.json");
    client.BaseAddress = new Uri(baseUrl.Trim().TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
})
.AddHttpMessageHandler<BearerTokenHandler>(); // 👈 token adjuntado automáticamente

// 🔥 SERVICIOS
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IPerfilService, PerfilService>();
builder.Services.AddScoped<ISaldoService, SaldoService>();
builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();
builder.Services.AddScoped<AfiliacionService>();
builder.Services.AddScoped<MovimientoService>();

// 🔥 SESSION
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ⏱️ 5 minutos por defecto (o config)
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("SessionSettings:TimeoutMinutes", 5));
});

var app = builder.Build();

// 🔥 ERRORES
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// 🔥 PIPELINE CORRECTO
app.UseStaticFiles();
app.UseRouting();

// 🔥 SESSION SIEMPRE ANTES DEL MIDDLEWARE
app.UseSession();

// 🔥 DETECTOR DE SESIÓN EXPIRADA (CLAVE)
app.Use(async (context, next) =>
{
    var teniaSesion = context.Session.GetString("UsuarioId");

    await next();

    var tieneSesionAhora = context.Session.GetString("UsuarioId");

    if (!string.IsNullOrEmpty(teniaSesion) && string.IsNullOrEmpty(tieneSesionAhora))
    {
        context.Session.SetString("SessionExpiredMessage", "La sesión expiró por inactividad.");
    }
});

app.UseAuthorization();

app.MapRazorPages();
app.Run();