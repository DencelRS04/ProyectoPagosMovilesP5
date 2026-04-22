namespace PagosMoviles.AdminWeb.Models.Cuentas
{
    public class CuentaEditModel
    {
        public int CuentaId { get; set; }
        public string NumeroCuenta { get; set; }
        public string TipoCuenta { get; set; }
        public decimal Saldo { get; set; }
    }
}