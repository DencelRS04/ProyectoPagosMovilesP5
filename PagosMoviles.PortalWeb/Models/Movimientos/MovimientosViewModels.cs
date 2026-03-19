namespace PagosMoviles.PortalWeb.Models.Movimientos
{
    public class MovimientosResponse
    {
        public object? Afiliacion { get; set; }
        public List<MovimientosViewModels> Movimientos { get; set; } = new();
    }

    public class MovimientosViewModels
    {
        public string TipoMovimiento { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
    }
}