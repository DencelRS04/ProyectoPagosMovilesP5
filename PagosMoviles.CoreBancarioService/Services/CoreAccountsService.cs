using PagosMoviles.CoreBancarioService.Data;
using PagosMoviles.CoreBancarioService.DTOs;
using PagosMoviles.CoreBancarioService.Models;

namespace PagosMoviles.CoreBancarioService.Services
{
    public class CoreAccountsService
    {
        private readonly CoreDbContext _db;

        public CoreAccountsService(CoreDbContext db)
        {
            _db = db;
        }

        // Listar todas
        public ApiResponse<List<Cuenta>> ListarTodas()
        {
            var cuentas = _db.Cuentas.ToList();
            return ApiResponse<List<Cuenta>>.Ok(cuentas, "Cuentas obtenidas");
        }

        // Listar por llave primaria
        public ApiResponse<Cuenta> ListarPorId(int id)
        {
            var cuenta = _db.Cuentas.FirstOrDefault(c => c.CuentaId == id);
            if (cuenta == null)
                return ApiResponse<Cuenta>.Fail("Cuenta no encontrada");

            return ApiResponse<Cuenta>.Ok(cuenta, "Cuenta encontrada");
        }

        // Listar por cliente
        public ApiResponse<List<Cuenta>> ListarPorCliente(int clienteId)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == clienteId);
            if (cliente == null)
                return ApiResponse<List<Cuenta>>.Fail("Cliente no existe");

            var cuentas = _db.Cuentas.Where(c => c.ClienteId == clienteId).ToList();
            return ApiResponse<List<Cuenta>>.Ok(cuentas, "Cuentas del cliente obtenidas");
        }

        // Crear
        public ApiResponse<Cuenta> Crear(CrearCuentaDto dto)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == dto.ClienteId);
            if (cliente == null)
                return ApiResponse<Cuenta>.Fail("Cliente no existe");

            bool duplicado = _db.Cuentas.Any(c => c.NumeroCuenta == dto.NumeroCuenta.Trim());
            if (duplicado)
                return ApiResponse<Cuenta>.Fail("El número de cuenta ya existe");

            var cuenta = new Cuenta
            {
                ClienteId = dto.ClienteId,
                NumeroCuenta = dto.NumeroCuenta.Trim(),
                Saldo = dto.Saldo
            };

            _db.Cuentas.Add(cuenta);
            _db.SaveChanges();

            return ApiResponse<Cuenta>.Ok(cuenta, "Cuenta creada");
        }

        // Editar
        public ApiResponse<Cuenta> Editar(int id, EditarCuentaDto dto)
        {
            var cuenta = _db.Cuentas.FirstOrDefault(c => c.CuentaId == id);
            if (cuenta == null)
                return ApiResponse<Cuenta>.Fail("Cuenta no encontrada");

            bool duplicado = _db.Cuentas.Any(c =>
                c.NumeroCuenta == dto.NumeroCuenta.Trim() && c.CuentaId != id);
            if (duplicado)
                return ApiResponse<Cuenta>.Fail("El número de cuenta ya existe en otra cuenta");

            var anterior = new { cuenta.NumeroCuenta, cuenta.Saldo };

            cuenta.NumeroCuenta = dto.NumeroCuenta.Trim();
            cuenta.Saldo = dto.Saldo;

            _db.SaveChanges();

            return ApiResponse<Cuenta>.Ok(cuenta, "Cuenta actualizada");
        }

        // Eliminar
        public ApiResponse<bool> Eliminar(int id)
        {
            var cuenta = _db.Cuentas.FirstOrDefault(c => c.CuentaId == id);
            if (cuenta == null)
                return ApiResponse<bool>.Fail("Cuenta no encontrada");

            _db.Cuentas.Remove(cuenta);
            _db.SaveChanges();

            return ApiResponse<bool>.Ok(true, "Cuenta eliminada");
        }
    }
}