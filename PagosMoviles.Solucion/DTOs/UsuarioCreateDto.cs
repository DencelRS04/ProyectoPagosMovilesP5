using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.UsuariosService.DTOs
{
    public class UsuarioCreateDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres")]
        public string Password { get; set; } = string.Empty;
    }
}
