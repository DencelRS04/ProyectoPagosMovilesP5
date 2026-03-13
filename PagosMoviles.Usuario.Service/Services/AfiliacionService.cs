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

        public AfiliacionService(AppDbContext context, CoreClientService coreService)
        {
            _context = context;
            _coreService = coreService;
        }

        public ApiResponse<PagoMovil> Inscribir(PagoMovilDto dto)
        {
            // 1️⃣ Validar que usuario exista
            var usuario = _context.Usuarios.FirstOrDefault(u => u.UsuarioId == dto.UsuarioId);

            if (usuario == null)
                return ApiResponse<PagoMovil>.Fail("El usuario no existe.");

            // 2️⃣ Validar que exista en Core
            var coreResponse = _coreService.VerificarClienteEnCore(usuario.Identificacion);

            if (!coreResponse.Success)
                return ApiResponse<PagoMovil>.Fail("El cliente no existe en el core bancario.");

            // 3️⃣ Validar que la cuenta no esté ya registrada
            if (_context.PagoMoviles.Any(p => p.NumeroCuenta == dto.NumeroCuenta && p.Estado))
                return ApiResponse<PagoMovil>.Fail("La cuenta ya está afiliada a Pago Móvil.");

            // 4️⃣ Validar que la identificación no esté ya inscrita
            if (_context.PagoMoviles.Any(p => p.Identificacion == usuario.Identificacion && p.Estado))
                return ApiResponse<PagoMovil>.Fail("Este cliente ya está inscrito en Pago Móvil.");

            // 5️⃣ Validar que el teléfono no esté usado
            if (_context.PagoMoviles.Any(p => p.Telefono == dto.Telefono && p.Estado))
                return ApiResponse<PagoMovil>.Fail("El número de teléfono ya está asociado a otro cliente.");

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

            return ApiResponse<PagoMovil>.Ok(pagoMovil, "Inscripción realizada correctamente.");
        }


    }
}
