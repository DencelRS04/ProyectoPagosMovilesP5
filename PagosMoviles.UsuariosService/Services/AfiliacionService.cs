using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Models;
using PagosMoviles.UsuariosService.Utils;

namespace PagosMoviles.UsuariosService.Services
{
    public class AfiliacionService
    {
        private readonly AppDbContext _context;
        private readonly CoreClientService _coreService;
        private readonly BitacoraClient _bitacora;

        public AfiliacionService(AppDbContext context, CoreClientService coreService, BitacoraClient bitacora)
        {
            _context = context;
            _coreService = coreService;
            _bitacora = bitacora;
        }

        public async Task<ApiResponse<PagoMovil>> Inscribir(PagoMovilDto dto)
        {
            // 1?? Validar que usuario exista
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == dto.UsuarioId);

            if (usuario == null)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"Intento de inscripción fallida: usuario {dto.UsuarioId} no existe");
                return ApiResponse<PagoMovil>.Fail("El usuario no existe.");
            }

            // 2?? Validar que exista en Core
            var coreResponse = _coreService.VerificarClienteEnCore(usuario.Identificacion);

            if (!coreResponse.Success)
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"Intento de inscripción fallida: cliente {usuario.Identificacion} no existe en core");
                return ApiResponse<PagoMovil>.Fail("El cliente no existe en el core bancario.");
            }

            // 3?? Validar que la cuenta no esté ya registrada
            if (_context.PagoMoviles.Any(p => p.NumeroCuenta == dto.NumeroCuenta && p.Estado))
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"Intento de inscripción fallida: cuenta {dto.NumeroCuenta} ya afiliada");
                return ApiResponse<PagoMovil>.Fail("La cuenta ya está afiliada a Pago Móvil.");
            }

            // 4?? Validar que la identificación no esté ya inscrita
            if (_context.PagoMoviles.Any(p => p.Identificacion == usuario.Identificacion && p.Estado))
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"Intento de inscripción fallida: cliente {usuario.Identificacion} ya inscrito");
                return ApiResponse<PagoMovil>.Fail("Este cliente ya está inscrito en Pago Móvil.");
            }

            // 5?? Validar que el teléfono no esté usado
            if (_context.PagoMoviles.Any(p => p.Telefono == dto.Telefono && p.Estado))
            {
                await _bitacora.RegistrarAsync("SYSTEM", $"Intento de inscripción fallida: teléfono {dto.Telefono} ya en uso");
                return ApiResponse<PagoMovil>.Fail("El número de teléfono ya está asociado a otro cliente.");
            }

            var pagoMovil = new PagoMovil
            {
                Identificacion = usuario.Identificacion,
                NumeroCuenta = dto.NumeroCuenta,
                Telefono = dto.Telefono,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.PagoMoviles.Add(pagoMovil);
            _context.SaveChanges();

            // ? Registrar inscripción exitosa
            await _bitacora.RegistrarAsync(
                "SYSTEM", 
                $"Inscripción exitosa",
                null,
                new { pagoMovil.Identificacion, pagoMovil.Telefono, pagoMovil.NumeroCuenta }
            );

            return ApiResponse<PagoMovil>.Ok(pagoMovil, "Inscripción realizada correctamente.");
        }
    }
}