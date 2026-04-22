namespace PagosMoviles.AdminWeb.Models.Cuentas
{
    public class CuentaCreateModel
    {
        public int ClienteId { get; set; }
        public string NumeroCuenta { get; set; }
        public string TipoCuenta { get; set; }
        public decimal Saldo { get; set; }
    }
}