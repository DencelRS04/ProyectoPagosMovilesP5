using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.UsuariosService.DTOs
{
    public class PagoMovilDto
    {
        [Required(ErrorMessage = "El UsuarioId es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "UsuarioId inválido")]
        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El número de cuenta es obligatorio")]
        [MinLength(6, ErrorMessage = "El número de cuenta es muy corto")]
        [MaxLength(20, ErrorMessage = "El número de cuenta es muy largo")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos")]
        public string Telefono { get; set; } = string.Empty;
    }
}
