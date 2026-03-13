using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Models;
using PagosMoviles.UsuariosService.Utils;

namespace PagosMoviles.UsuariosService.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<Usuario>> CrearUsuarioAsync(UsuarioCreateDto dto)
        {
            if (dto == null)
                return ApiResponse<Usuario>.Fail("Datos incorrectos.");

            var email = dto.Email?.Trim();
            var identificacion = dto.Identificacion?.Trim();
            var tipoId = dto.TipoIdentificacion?.Trim();
            var nombre = dto.NombreCompleto?.Trim();
            var telefono = dto.Telefono?.Trim();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(identificacion) ||
                string.IsNullOrWhiteSpace(tipoId) ||
                string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return ApiResponse<Usuario>.Fail("Debe enviar los datos completos y válidos.");
            }

            var emailExists = await _context.Usuarios.AsNoTracking().AnyAsync(u => u.Email == email);
            if (emailExists)
                return ApiResponse<Usuario>.Fail("El email ya está registrado.");

            var idExists = await _context.Usuarios.AsNoTracking().AnyAsync(u => u.Identificacion == identificacion);
            if (idExists)
                return ApiResponse<Usuario>.Fail("La identificación ya está registrada.");

            var usuario = new Usuario
            {
                Email = email,
                TipoIdentificacion = tipoId,
                Identificacion = identificacion,
                NombreCompleto = nombre,
                Telefono = telefono,
                RolId = dto.RolId,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return ApiResponse<Usuario>.Ok(usuario, "Usuario creado correctamente.");
        }

        public async Task<ApiResponse<Usuario>> ActualizarUsuarioAsync(int id, UsuarioUpdateDto dto)
        {
            if (dto == null)
                return ApiResponse<Usuario>.Fail("Datos incorrectos.");

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (usuario == null)
                return ApiResponse<Usuario>.Fail("Usuario no encontrado.");

            usuario.NombreCompleto = dto.NombreCompleto?.Trim() ?? usuario.NombreCompleto;
            usuario.Telefono = dto.Telefono?.Trim() ?? usuario.Telefono;
            usuario.RolId = dto.RolId;

            await _context.SaveChangesAsync();

            return ApiResponse<Usuario>.Ok(usuario, "Usuario actualizado correctamente.");
        }

        public async Task<ApiResponse<string>> EliminarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (usuario == null)
                return ApiResponse<string>.Fail("Usuario no encontrado.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Eliminado", "Usuario eliminado correctamente.");
        }

        public async Task<ApiResponse<List<Usuario>>> ObtenerTodosAsync()
        {
            var lista = await _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.UsuarioId)
                .ToListAsync();

            return ApiResponse<List<Usuario>>.Ok(lista);
        }

        public async Task<ApiResponse<Usuario>> ObtenerPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.UsuarioId == id);
            if (usuario == null)
                return ApiResponse<Usuario>.Fail("Usuario no encontrado.");

            return ApiResponse<Usuario>.Ok(usuario);
        }
    }
}