using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Models;

namespace PagosMoviles.CoreBancarioService.Services
{
    public class CoreTransaccionService
    {
        private readonly CoreDbContext _db;

        public CoreTransaccionService(CoreDbContext db)
        {
            _db = db;
        }

        private static string NormalizarTipo(string tipo) =>
            tipo.Trim().ToUpper() switch
            {
                "C" => "CREDITO",
                "D" => "DEBITO",
                "CREDITO" => "CREDITO",
                "DEBITO" => "DEBITO",
                _ => tipo.Trim().ToUpper()
            };


        // SRV14
        public ApiResponse<Movimiento> AplicarTransaccion(AplicarTransaccionDto dto)
        {
            var identificacion = dto.Identificacion.Trim();
            var numeroCuenta = dto.NumeroCuenta.Trim();

            // 1) Cliente existe
            var cliente = _db.Clientes.FirstOrDefault(c => c.Identificacion == identificacion);
            if (cliente == null)
                return ApiResponse<Movimiento>.Fail("Cliente no existe");

            // 2) Cuenta existe
            var cuenta = _db.Cuentas.FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
            if (cuenta == null)
                return ApiResponse<Movimiento>.Fail("Cuenta no existe");

            // 3) Cuenta pertenece al cliente
            if (cuenta.ClienteId != cliente.ClienteId)
                return ApiResponse<Movimiento>.Fail("La cuenta no pertenece al cliente");

            // 4) Tipo normalizado
            var tipoNormalizado = NormalizarTipo(dto.Tipo);

            // 5) Saldo
            decimal nuevoSaldo = cuenta.Saldo + (tipoNormalizado == "CREDITO" ? dto.Monto : -dto.Monto);
            if (tipoNormalizado == "DEBITO" && nuevoSaldo < 0)
                return ApiResponse<Movimiento>.Fail("Fondos insuficientes");

            cuenta.Saldo = nuevoSaldo;

            var mov = new Movimiento
            {
                CuentaId = cuenta.CuentaId,
                TipoMovimiento = tipoNormalizado,
                Monto = dto.Monto,
                Fecha = DateTime.Now
            };

            _db.Movimientos.Add(mov);
            _db.SaveChanges();

            return ApiResponse<Movimiento>.Ok(mov, "Transacción aplicada");
        }


        // SRV16
        public ApiResponse<List<Movimiento>> UltimosMovimientos(string identificacion, string numeroCuenta)
        {
            identificacion = identificacion.Trim();
            numeroCuenta = numeroCuenta.Trim();

            var cliente = _db.Clientes.FirstOrDefault(c => c.Identificacion == identificacion);
            if (cliente == null)
                return ApiResponse<List<Movimiento>>.Fail("Cliente no existe");

            var cuenta = _db.Cuentas.FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
            if (cuenta == null)
                return ApiResponse<List<Movimiento>>.Fail("Cuenta no existe");

            if (cuenta.ClienteId != cliente.ClienteId)
                return ApiResponse<List<Movimiento>>.Fail("La cuenta no pertenece al cliente");

            var lista = _db.Movimientos
                .Where(m => m.CuentaId == cuenta.CuentaId)
                .OrderByDescending(m => m.Fecha)
                .Take(5)
                .ToList();

            return ApiResponse<List<Movimiento>>.Ok(lista, "Movimientos consultados");
        }

    }
}
