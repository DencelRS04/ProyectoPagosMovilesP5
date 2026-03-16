namespace PagosMoviles.AdminWeb.Models.Transacciones
{
    public class TransacionViewModel
    {
        
        public DateTime Fecha { get; set; }

        public string TelefonoOrigen { get; set; }

        public string TelefonoDestino { get; set; }

        public decimal Monto { get; set; }
    
    }
}
