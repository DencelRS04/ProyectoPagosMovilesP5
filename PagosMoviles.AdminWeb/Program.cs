using PagosMoviles.AdminWeb.Services.ClientesCore;
using PagosMoviles.AdminWeb.Services.Entidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddSession();

builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7130/");
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7205/");
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