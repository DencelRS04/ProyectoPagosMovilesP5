var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddReverseProxy()  // ← Esto es clave
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔹 No Authorization por ahora
// app.UseAuthorization();

app.MapControllers();
app.MapReverseProxy();  // ← Funciona solo si AddReverseProxy está agregado

app.Run();