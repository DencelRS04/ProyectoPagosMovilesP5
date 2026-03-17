using PagosMoviles.PortalWeb.Services.Auth;
using PagosMoviles.PortalWeb.Services.Perfil;
using PagosMoviles.PortalWeb.Services.Saldo;
using PagosMoviles.PortalWeb.Services.Transferencias;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

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