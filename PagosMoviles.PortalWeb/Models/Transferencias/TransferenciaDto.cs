namespace PagosMoviles.PortalWeb.Models.Transferencias
{
    public class TransferenciaRequestDto
    {
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public string EntidadDestino { get; set; } = string.Empty;
        public string EntidadOrigen { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class TransferenciaResponseDto
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}