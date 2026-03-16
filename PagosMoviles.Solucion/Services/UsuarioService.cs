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

        public async Task<ServiceResponse<List<UsuarioDto>>> ObtenerTodosAsync()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new UsuarioDto
                {
                    UsuarioId = u.UsuarioId,
                    Email = u.Email,
                    TipoIdentificacion = u.TipoIdentificacion,
                    Identificacion = u.Identificacion,
                    NombreCompleto = u.NombreCompleto,
                    Telefono = u.Telefono,
                    RolId = u.RolId,
                    PasswordHash = u.PasswordHash,
                    FechaCreacion = u.FechaCreacion,
                    FotoPerfil = u.FotoPerfil,
                    ColorAvatar = string.IsNullOrWhiteSpace(u.ColorAvatar) ? "#4285F4" : u.ColorAvatar
                })
                .ToListAsync();

            return new ServiceResponse<List<UsuarioDto>>
            {
                Success = true,
                Message = "Consulta exitosa.",
                Data = usuarios
            };
        }

        public async Task<ServiceResponse<UsuarioDto>> ObtenerPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.UsuarioId == id)
                .Select(u => new UsuarioDto
                {
                    UsuarioId = u.UsuarioId,
                    Email = u.Email,
                    TipoIdentificacion = u.TipoIdentificacion,
                    Identificacion = u.Identificacion,
                    NombreCompleto = u.NombreCompleto,
                    Telefono = u.Telefono,
                    RolId = u.RolId,
                    PasswordHash = u.PasswordHash,
                    FechaCreacion = u.FechaCreacion,
                    FotoPerfil = u.FotoPerfil,
                    ColorAvatar = string.IsNullOrWhiteSpace(u.ColorAvatar) ? "#4285F4" : u.ColorAvatar
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return new ServiceResponse<UsuarioDto>
                {
                    Success = false,
                    Message = "Usuario no encontrado.",
                    Data = null
                };
            }

            return new ServiceResponse<UsuarioDto>
            {
                Success = true,
                Message = "Consulta exitosa.",
                Data = usuario
            };
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
                FechaCreacion = DateTime.Now,
                FotoPerfil = string.Empty,
                ColorAvatar = "#4285F4"
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
    }
}