<<<<<<< HEAD
using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.PortalWeb.Services.Perfil;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.PortalWeb.Services.Transferencias;
=======
using PagosMoviles.PortalWeb.Services.Afiliacion;
>>>>>>> 5edba4e (Avance Portal web y afiliacion + Gateway)

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

<<<<<<< HEAD
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("gateway", client =>
{
    client.BaseAddress = new Uri("https://localhost:7143/");
});

builder.Services.AddHttpClient("InscripcionApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["InscripcionApiUrl"] ?? "https://localhost:7143/");
});

builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IPerfilService, PerfilService>();

builder.Services.AddScoped<ISaldoService, SaldoService>();
builder.Services.AddScoped<ITransferenciaService, TransferenciaService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(
        builder.Configuration.GetValue<int>("SessionSettings:TimeoutMinutes", 5));
});
=======
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
});

builder.Services.AddScoped<AfiliacionService>();
>>>>>>> 5edba4e (Avance Portal web y afiliacion + Gateway)

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();