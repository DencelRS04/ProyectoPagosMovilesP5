using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.PortalWeb.Models.Afiliacion
{
    public class AfiliacionViewModel
    {
        [Required(ErrorMessage = "La identificación es obligatoria")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "La identificación debe tener 9 dígitos")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [RegularExpression(@"^[A-Za-z0-9]{3,25}$", ErrorMessage = "El número de cuenta es inválido")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
        public bool ClienteExiste { get; set; }
    }
}