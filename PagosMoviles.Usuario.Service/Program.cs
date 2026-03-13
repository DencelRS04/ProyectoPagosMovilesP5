using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext (PagosMoviles)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// HttpClient para CoreBancarioService (SRV19 real por HTTP)
builder.Services.AddHttpClient("CoreBancario", client =>
{
    // ?? Cambiá este puerto por el que te salga en el CoreBancarioService
    client.BaseAddress = new Uri("https://localhost:7028/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Services
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<AfiliacionService>();
builder.Services.AddScoped<CoreClientService>();

var app = builder.Build();

// Swagger solo en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// Si todavía no estás usando JWT, esto no hace nada pero no estorba.
// Cuando tengas auth, aquí va app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
