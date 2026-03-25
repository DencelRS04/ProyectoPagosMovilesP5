using Microsoft.EntityFrameworkCore;
using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Models;

namespace PagosMoviles.CoreBancarioService.Services
{
    public class CoreClienteService
    {
        private readonly CoreDbContext _db;

        public CoreClienteService(CoreDbContext db)
        {
            _db = db;
        }

        public async Task<List<ClienteDto>> ListarAsync()
        {
            return await _db.Clientes
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.ClienteId)
                .Select(c => new ClienteDto
                {
                    ClienteId = c.ClienteId,
                    Identificacion = c.Identificacion,
                    TipoIdentificacion = c.TipoIdentificacion,
                    NombreCompleto = c.NombreCompleto,
                    FechaNacimiento = c.FechaNacimiento,
                    Telefono = c.Telefono,
                    Activo = c.Activo
                })
                .ToListAsync();
        }

        public async Task<ClienteDto?> ObtenerPorIdAsync(int id)
        {
            return await _db.Clientes
                .AsNoTracking()
                .Where(c => c.ClienteId == id)
                .Select(c => new ClienteDto
                {
                    ClienteId = c.ClienteId,
                    Identificacion = c.Identificacion,
                    TipoIdentificacion = c.TipoIdentificacion,
                    NombreCompleto = c.NombreCompleto,
                    FechaNacimiento = c.FechaNacimiento,
                    Telefono = c.Telefono,
                    Activo = c.Activo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool ok, string mensaje, object? cliente)> CrearAsync(ClienteDto dto)
{
    try
    {
        // Validar duplicado
        var existe = await _db.Clientes
            .AnyAsync(c => c.Identificacion == dto.Identificacion);

        if (existe)
            return (false, "La identificación ya existe", null);

        // Crear entidad
        var cliente = new Cliente
        {
            Identificacion = dto.Identificacion,
            TipoIdentificacion = dto.TipoIdentificacion,
            NombreCompleto = dto.NombreCompleto,
            FechaNacimiento = dto.FechaNacimiento,
            Telefono = dto.Telefono,
            Activo = true
        };

        //  AGREGAR
        _db.Clientes.Add(cliente);

        //  GUARDAR
        await _db.SaveChangesAsync();

        return (true, "Cliente creado correctamente", cliente);
    }
    catch (Exception ex)
    {
        return (false, $"Error: {ex.Message}", null);
    }
}

        public async Task<bool> ExistePorIdentificacionAsync(string identificacion)
        {
            return await _db.Clientes
                .AsNoTracking()
                .AnyAsync(c => c.Identificacion == identificacion && c.Activo);
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(int id, ClienteDto dto)
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null)
                return (false, "Cliente no encontrado");

            bool identificacionDuplicada = await _db.Clientes
                .AnyAsync(c => c.Identificacion == dto.Identificacion && c.ClienteId != id);

            if (identificacionDuplicada)
                return (false, "Ya existe otro cliente con esa identificación");

            cliente.Identificacion = dto.Identificacion;
            cliente.TipoIdentificacion = dto.TipoIdentificacion;
            cliente.NombreCompleto = dto.NombreCompleto;
            cliente.FechaNacimiento = dto.FechaNacimiento;
            cliente.Telefono = dto.Telefono;
            cliente.Activo = dto.Activo;

            await _db.SaveChangesAsync();
            return (true, "Cliente actualizado correctamente");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var cliente = await _db.Clientes.FirstOrDefaultAsync(c => c.ClienteId == id);
            if (cliente == null)
                return (false, "Cliente no encontrado");

            if (!cliente.Activo)
                return (false, "El cliente ya está inactivo");

            cliente.Activo = false;
            await _db.SaveChangesAsync();

            return (true, "Cliente desactivado correctamente");
        }
    }
}