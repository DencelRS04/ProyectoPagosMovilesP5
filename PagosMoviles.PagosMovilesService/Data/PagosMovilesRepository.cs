using Dapper;
using Microsoft.Data.SqlClient;
using PagosMoviles.PagosMovilesService.Dtos;

namespace PagosMoviles.PagosMovilesService.Data
{
    public class PagosMovilesRepository
    {
        private readonly string _cnPagos;
        private readonly string _cnCore;

        public PagosMovilesRepository(IConfiguration cfg)
        {
            _cnPagos = cfg.GetConnectionString("PagosMovilesDb")!;
            _cnCore = cfg.GetConnectionString("CoreBancarioDb")!;
        }

        private SqlConnection OpenPagos() => new SqlConnection(_cnPagos);
        private SqlConnection OpenCore() => new SqlConnection(_cnCore);

        // =====================================================
        // PAGO MOVIL - Afiliación por teléfono (SRV13/SRV11)
        // =====================================================
        public async Task<PagoMovilMiniDto?> GetPagoMovilMiniByTelefonoAsync(string telefono)
        {
            using var db = OpenPagos();

            const string sql = @"
SELECT TOP 1 Identificacion, NumeroCuenta, Telefono, Estado
FROM PagoMovil
WHERE Telefono = @telefono;
";
            return await db.QueryFirstOrDefaultAsync<PagoMovilMiniDto>(sql, new { telefono });
        }

        public async Task<PagoMovilMiniDto?> GetPagoMovilMiniByTelefonoIdentificacionAsync(string telefono, string identificacion)
        {
            using var db = OpenPagos();

            const string sql = @"
SELECT TOP 1 Identificacion, NumeroCuenta, Telefono, Estado
FROM PagoMovil
WHERE Telefono = @telefono
  AND Identificacion = @identificacion;
";
            return await db.QueryFirstOrDefaultAsync<PagoMovilMiniDto>(sql, new { telefono, identificacion });
        }

        // =====================================================
        // SRV10 - Cancelar suscripción (Estado=0)
        // =====================================================
        public async Task<int> CancelarSuscripcionAsync(string telefono, string identificacion, string numeroCuenta)
        {
            using var db = OpenPagos();

            const string sql = @"
UPDATE PagoMovil
SET Estado = 0
WHERE Telefono = @telefono
  AND Identificacion = @identificacion
  AND NumeroCuenta = @numeroCuenta
  AND Estado = 1;
";
            return await db.ExecuteAsync(sql, new { telefono, identificacion, numeroCuenta });
        }

        // =====================================================
        // SRV13 - Consultar saldo (CoreBancario DB directo)
        // =====================================================
        public async Task<decimal?> GetSaldoAsync(string identificacion, string numeroCuenta)
        {
            using var db = OpenCore();

            const string sql = @"
SELECT c.Saldo
FROM Cuenta c
INNER JOIN Cliente cl ON cl.ClienteId = c.ClienteId
WHERE cl.Identificacion = @identificacion
  AND c.NumeroCuenta = @numeroCuenta;
";
            return await db.QueryFirstOrDefaultAsync<decimal?>(sql, new { identificacion, numeroCuenta });
        }

        // =====================================================
        // SRV11 - Últimos 5 movimientos (CoreBancario DB directo)
        // =====================================================
        public async Task<List<(string TipoMovimiento, decimal Monto, DateTime Fecha)>> GetUltimos5MovimientosAsync(
            string identificacion,
            string numeroCuenta)
        {
            using var db = OpenCore();

            const string sql = @"
SELECT TOP 5 m.TipoMovimiento, m.Monto, m.Fecha
FROM Movimiento m
INNER JOIN Cuenta c ON c.CuentaId = m.CuentaId
INNER JOIN Cliente cl ON cl.ClienteId = c.ClienteId
WHERE cl.Identificacion = @identificacion
  AND c.NumeroCuenta = @numeroCuenta
ORDER BY m.Fecha DESC;
";
            var rows = await db.QueryAsync(sql, new { identificacion, numeroCuenta });
            return rows.Select(r => ((string)r.TipoMovimiento, (decimal)r.Monto, (DateTime)r.Fecha)).ToList();
        }

        // =====================================================
        // SRV17 - Reporte diario (PagosMoviles DB)
        // =====================================================
        public async Task<List<TransaccionMovilDto>> GetTransaccionesDiariasAsync(DateTime fecha)
        {
            using var db = OpenPagos();

            const string sql = @"
SELECT 
    TransaccionId,
    EntidadOrigen,
    EntidadDestino,
    TelefonoOrigen,
    TelefonoDestino,
    Monto,
    Descripcion,
    Fecha
FROM TransaccionMovil
WHERE CAST(Fecha AS DATE) = @fecha
ORDER BY Fecha DESC;
";
            var rows = await db.QueryAsync<TransaccionMovilDto>(sql, new { fecha = fecha.Date });
            return rows.ToList();
        }

        public async Task<(int Cantidad, decimal Total)> GetTotalesTransaccionesDiariasAsync(DateTime fecha)
        {
            using var db = OpenPagos();

            const string sql = @"
SELECT 
    COUNT(*) AS Cantidad,
    ISNULL(SUM(Monto), 0) AS Total
FROM TransaccionMovil
WHERE CAST(Fecha AS DATE) = @fecha;
";
            var row = await db.QueryFirstAsync(sql, new { fecha = fecha.Date });
            return ((int)row.Cantidad, (decimal)row.Total);
        }

        // =====================================================
        // Bitácora por BD (si la estás usando)
        // =====================================================
        public async Task RegistrarBitacoraAsync(string usuario, string descripcion)
        {
            using var db = OpenPagos();

            const string sql = @"
INSERT INTO Bitacora (Usuario, Descripcion)
VALUES (@usuario, @descripcion);
";
            await db.ExecuteAsync(sql, new { usuario, descripcion });
        }
    }
}