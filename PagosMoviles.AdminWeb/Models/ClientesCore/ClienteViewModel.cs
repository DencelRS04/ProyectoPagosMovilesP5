using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Models.ClientesCore
{
    public class ClienteViewModel
    {
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "La identificación es requerida")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de identificación es requerido")]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre completo es requerido")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El teléfono es requerido")]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}