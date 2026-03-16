using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.PortalWeb.Models.Afiliacion
{
    public class AfiliacionViewModel
    {
        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de teléfono es requerido")]
        public string Telefono { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
        public bool ClienteExiste { get; set; }
    }
}