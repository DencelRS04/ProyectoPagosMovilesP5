using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Models.ClientesCore
{
    public class ClienteCreateModel
    {
        [Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}