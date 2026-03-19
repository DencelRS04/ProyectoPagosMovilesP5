var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// ✅ Una sola vez, con BaseAddress correcta
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

// ✅ Una sola vez
builder.Services.AddScoped<MovimientoService>();

var app = builder.Build();

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