using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.UsuariosService.DTOs
{
    public class UsuarioUpdateDto
    {
        [Required]
        [MinLength(3)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        public int IntentosFallidos { get; set; }

        public bool Bloqueado { get; set; }
    }
}
