namespace PagosMoviles.AdminWeb.Models.Cuentas
{
    public class CuentaViewModel
    {
        public int CuentaId { get; set; }
        public int ClienteId { get; set; }
        public string NumeroCuenta { get; set; }
        public string TipoCuenta { get; set; }
        public decimal Saldo { get; set; }
    }
}