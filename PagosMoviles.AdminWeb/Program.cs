using PagosMoviles.AdminWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient("UsuarioApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["UsuarioApiUrl"] ?? "https://localhost:7154/");
});

builder.Services.AddHttpClient("ParametroApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ParametroApiUrl"] ?? "https://localhost:7143/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();



app.UseSession();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
