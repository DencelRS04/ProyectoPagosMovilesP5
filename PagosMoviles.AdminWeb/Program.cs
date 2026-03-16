using PagosMoviles.AdminWeb.Services;
using PagosMoviles.AdminWeb.Services.Auth;
using PagosMoviles.AdminWeb.Services.Perfil;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("UsuarioApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UsuarioApiUrl"] ?? "https://localhost:7154/");
});

builder.Services.AddHttpClient("ParametroApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ParametroApiUrl"] ?? "https://localhost:7143/");
});

builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IPerfilService, PerfilService>();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/Home/Index");
        return Task.CompletedTask;
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();