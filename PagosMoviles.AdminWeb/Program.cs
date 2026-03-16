using PagosMoviles.AdminWeb.Services.ClientesCore;
using PagosMoviles.AdminWeb.Services.Entidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSession();

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

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();