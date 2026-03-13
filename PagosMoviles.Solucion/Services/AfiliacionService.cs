using Microsoft.EntityFrameworkCore;
using PagosMoviles.UsuariosService.Data;
using PagosMoviles.UsuariosService.DTOs;
using PagosMoviles.UsuariosService.Models;
using PagosMoviles.UsuariosService.Utils;

namespace PagosMoviles.UsuariosService.Services
{
    public class AfiliacionService
    {
        private readonly AppDbContext _context;
        private readonly CoreClientService _core;

        public AfiliacionService(AppDbContext context, CoreClientService core)
        {
            _context = context;
            _core = core;
        }

        // SRV9 (según documento): recibe Identificacion + NumeroCuenta + Telefono
        public async Task<SrvResponse<PagoMovil>> RegisterAsync(RegisterPagoMovilDto dto)
        {
            // (ModelState valida, pero por si llega sucio)
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Identificacion) ||
                string.IsNullOrWhiteSpace(dto.NumeroCuenta) ||
                string.IsNullOrWhiteSpace(dto.Telefono))
            {
                return SrvResponse<PagoMovil>.Fail("Datos incorrectos");
            }

            var identificacion = dto.Identificacion.Trim();
            var numeroCuenta = dto.NumeroCuenta.Trim();
            var telefono = dto.Telefono.Trim();

            // 1) Cliente debe existir en el Core (SRV19)
            var coreResp = await _core.VerificarClienteEnCoreAsync(identificacion);
            if (coreResp.Codigo != 0)
                return SrvResponse<PagoMovil>.Fail("Cliente no encontrado en el core");

            // 2) Si el teléfono ya está asociado a otra cuenta activa → error exacto
            var telActivo = await _context.PagoMoviles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Telefono == telefono && p.Estado);

            if (telActivo != null &&
                (telActivo.NumeroCuenta != numeroCuenta || telActivo.Identificacion != identificacion))
            {
                return SrvResponse<PagoMovil>.Fail("Teléfono ya se encuentra afiliado, realice el proceso de desinscripción");
            }

            // 3) Si ya existe registro exacto pero está deshabilitado → reactivar
            var existente = await _context.PagoMoviles
                .FirstOrDefaultAsync(p =>
                    p.Identificacion == identificacion &&
                    p.NumeroCuenta == numeroCuenta &&
                    p.Telefono == telefono);

            if (existente != null)
            {
                if (!existente.Estado)
                {
                    existente.Estado = true;
                    existente.FechaRegistro = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return SrvResponse<PagoMovil>.Ok(existente, "Inscripción realizada");
                }

                // Ya estaba activo
                return SrvResponse<PagoMovil>.Fail("El monedero ya se encuentra habilitado");
            }

            // 4) Si la cuenta ya está afiliada activa con otro teléfono/identificación → evita duplicados
            var cuentaAfiliadaActiva = await _context.PagoMoviles
                .AsNoTracking()
                .AnyAsync(p => p.NumeroCuenta == numeroCuenta && p.Estado);

            if (cuentaAfiliadaActiva)
                return SrvResponse<PagoMovil>.Fail("La cuenta ya está afiliada a Pago Móvil");

            // 5) Insertar
            var nuevo = new PagoMovil
            {
                Identificacion = identificacion,
                NumeroCuenta = numeroCuenta,
                Telefono = telefono,
                Estado = true,
                FechaRegistro = DateTime.Now
            };

            _context.PagoMoviles.Add(nuevo);
            await _context.SaveChangesAsync();

            return SrvResponse<PagoMovil>.Ok(nuevo, "Inscripción realizada");
        }
    }
}