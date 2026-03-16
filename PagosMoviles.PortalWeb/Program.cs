var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Program.cs (PagosMoviles.PortalWeb) - fragmento a añadir/ajustar
builder.Services.AddHttpClient("GatewayApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7130/"); // URL de tu Gateway
    client.Timeout = TimeSpan.FromSeconds(15);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

// Registrar el servicio que usa IHttpClientFactory
builder.Services.AddScoped<MovimientoService>();

builder.Services.AddScoped<MovimientoService>();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("GatewayApi");
builder.Services.AddScoped<MovimientoService>();// ✅ AQUÍ

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();