using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.DTOs;

namespace PagosMoviles.CoreBancarioService.Services
{
    public class CoreCuentaService
    {
        private readonly CoreDbContext _db;

        public CoreCuentaService(CoreDbContext db)
        {
            _db = db;
        }

        // SRV19 soporte
        public ApiResponse<bool> ClienteExiste(string identificacion)
        {
            bool existe = _db.Clientes.Any(c => c.Identificacion == identificacion);

            return existe
                ? ApiResponse<bool>.Ok(true, "Cliente existe")
                : ApiResponse<bool>.Fail("Cliente no existe");
        }

        // SRV15
        public ApiResponse<decimal> ConsultarSaldo(string identificacion, string numeroCuenta)
        {
            identificacion = identificacion.Trim();
            numeroCuenta = numeroCuenta.Trim();

            var cliente = _db.Clientes.FirstOrDefault(c => c.Identificacion == identificacion);
            if (cliente == null)
                return ApiResponse<decimal>.Fail("Cliente no existe");

            var cuenta = _db.Cuentas.FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
            if (cuenta == null)
                return ApiResponse<decimal>.Fail("Cuenta no existe");

            if (cuenta.ClienteId != cliente.ClienteId)
                return ApiResponse<decimal>.Fail("La cuenta no pertenece al cliente");

            return ApiResponse<decimal>.Ok(cuenta.Saldo, "Saldo consultado");
        }

    }
}
