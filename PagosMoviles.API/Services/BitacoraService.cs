using PagosMoviles.API.Data;
using PagosMoviles.API.Models;
using System.Text.Json;

namespace PagosMoviles.API.Services;

public class BitacoraService
{
    private readonly PagosMovilesDbContext _context;

    public BitacoraService(PagosMovilesDbContext context)
    {
        _context = context;
    }

    public async Task Registrar(string usuario, string accion, object? anterior = null, object? actual = null)
    {
        string desc = accion;

        if (anterior != null)
            desc += $" | ANTERIOR: {JsonSerializer.Serialize(anterior)}";

        if (actual != null)
            desc += $" | ACTUAL: {JsonSerializer.Serialize(actual)}";

        var bitacora = new Bitacora
        {
            Fecha = DateTime.Now,
            Usuario = usuario,
            Descripcion = desc
        };

        _context.Bitacoras.Add(bitacora);
        await _context.SaveChangesAsync();
    }
}
