namespace PagosMoviles.AdminWeb.Models.ClientesCore
{
    public class ClienteViewModel
    {
        public int ClienteId { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string TipoIdentificacion { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public DateTime? FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}