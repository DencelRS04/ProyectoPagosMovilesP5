using PagosMoviles.AdminWeb.Services.ClientesCore;
using PagosMoviles.AdminWeb.Services.Entidades;
// Al inicio del Program.cs agregar
using PagosMoviles.AdminWeb.Services.Reporte; // ← según donde esté ReporteService.cs
using PagosMoviles.AdminWeb.Handlers;
using Microsoft.AspNetCore.Http;
using System;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// Session (necesario para guardar el token)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true; // ← ignora mayúsculas
});

// HttpContextAccessor (para leer la sesión desde el handler)
builder.Services.AddHttpContextAccessor();

// Registra el DelegatingHandler que añade el Bearer token
builder.Services.AddTransient<BearerTokenHandler>();

// HttpClient configurado para el Gateway y con el handler
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7130/"); // URL de tu Gateway
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
})
.AddHttpMessageHandler<BearerTokenHandler>();

// Servicios de la aplicación
builder.Services.AddScoped<ClientesCoreService>();
builder.Services.AddScoped<EntidadService>();
builder.Services.AddScoped<ReporteService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session debe estar antes de MapRazorPages/Endpoints
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();