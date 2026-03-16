using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.PortalWeb.Models
{
    public class AfiliacionInputModel
    {
        [Required(ErrorMessage = "La identificación es obligatoria")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "La identificación debe tener 9 dígitos")]
        public string Identificacion { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [RegularExpression(@"^[A-Za-z0-9]{3,20}$", ErrorMessage = "Número de cuenta inválido")]
        public string NumeroCuenta { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; }
    }
}