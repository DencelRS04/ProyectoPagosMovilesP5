namespace PagosMoviles.AdminWeb.Models.Entidades
{
    public class TransaccionViewModel
    {
        public DateTime Fecha { get; set; }
        public string TelefonoOrigen { get; set; }
        public string TelefonoDestino { get; set; }
        public decimal Monto { get; set; }
    }
}