using PagosMoviles.AdminWeb.Models.Entidades;

namespace PagosMoviles.AdminWeb.Models.Transacciones
{
    public class TransacionViewModel
    {
        public List<TransaccionViewModel> Transacciones { get; set; } = new();
        //                                     ↑
        //                              Se llama "Transacciones"
        public DateTime Fecha { get; set; }

        public string TelefonoOrigen { get; set; }

        public string TelefonoDestino { get; set; }

        public decimal Monto { get; set; }
    
    }
}
