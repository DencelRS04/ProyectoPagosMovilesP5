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

        public ApiResponse<Usuario> CrearUsuario(UsuarioCreateDto dto)
        {
            // Validación duplicado email
            if (_context.Usuarios.Any(u => u.Email == dto.Email))
                return ApiResponse<Usuario>.Fail("El email ya está registrado.");

            // Validación duplicado identificación
            if (_context.Usuarios.Any(u => u.Identificacion == dto.Identificacion))
                return ApiResponse<Usuario>.Fail("La identificación ya está registrada.");

            var usuario = new Usuario
            {
                Email = dto.Email,
                TipoIdentificacion = dto.TipoIdentificacion,
                Identificacion = dto.Identificacion,
                NombreCompleto = dto.NombreCompleto,
                Telefono = dto.Telefono,
                RolId = dto.RolId,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return ApiResponse<Usuario>.Ok(usuario, "Usuario creado correctamente.");
        }

        public ApiResponse<Usuario> ActualizarUsuario(int id, UsuarioUpdateDto dto)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);

            if (usuario == null)
                return ApiResponse<Usuario>.Fail("Usuario no encontrado.");

            usuario.NombreCompleto = dto.NombreCompleto;
            usuario.Telefono = dto.Telefono;
            usuario.RolId = dto.RolId;

            _context.SaveChanges();

            return ApiResponse<Usuario>.Ok(usuario, "Usuario actualizado correctamente.");
        }

        public ApiResponse<string> EliminarUsuario(int id)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);

            if (usuario == null)
                return ApiResponse<string>.Fail("Usuario no encontrado.");

            _context.Usuarios.Remove(usuario);
            _context.SaveChanges();

            return ApiResponse<string>.Ok("Eliminado", "Usuario eliminado correctamente.");
        }

        public ApiResponse<List<Usuario>> ObtenerTodos()
        {
            var lista = _context.Usuarios.ToList();
            return ApiResponse<List<Usuario>>.Ok(lista);
        }

        public ApiResponse<Usuario> ObtenerPorId(int id)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == id);

            if (usuario == null)
                return ApiResponse<Usuario>.Fail("Usuario no encontrado.");

            return ApiResponse<Usuario>.Ok(usuario);
        }
    }
}
