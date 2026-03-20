using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.PortalWeb.Services.Perfil;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.PortalWeb.Services.Transferencias;
using PagosMoviles.PortalWeb.Services.Afiliacion;
using PagosMoviles.PortalWeb.Handlers;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Razor + contexto HTTP
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// ✅ Registrar handler (IMPORTANTE: antes de usarlo)
builder.Services.AddTransient<BearerTokenHandler>();

// 🔥 HttpClient principal (Gateway)
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
.AddHttpMessageHandler<BearerTokenHandler>();

// 🔥 Servicios
builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IPerfilService, PerfilService>();
builder.Services.AddScoped<ISaldoService, SaldoService>();
builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();
builder.Services.AddScoped<AfiliacionService>();
builder.Services.AddScoped<MovimientoService>();

// 🔥 Sesión
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("SessionSettings:TimeoutMinutes", 5));
});

var app = builder.Build();

// 🔥 Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // 👈 IMPORTANTE antes de Authorization

app.UseAuthorization();

app.MapRazorPages();

app.Run();