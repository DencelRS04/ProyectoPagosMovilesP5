using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.CoreBancarioService.DTOs
{
    public class ClienteDto
    {
        public int ClienteId { get; set; }

        [Required]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}