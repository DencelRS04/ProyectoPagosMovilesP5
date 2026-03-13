using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.UsuariosService.DTOs
{
    public class RegisterPagoMovilDto
    {
        [Required(ErrorMessage = "Identificación requerida")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Identificación inválida (solo números, 9-12 dígitos)")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número de cuenta requerido")]
        [RegularExpression(@"^[A-Za-z0-9]{3,30}$", ErrorMessage = "Número de cuenta inválido (solo letras y números, 3-30)")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teléfono requerido")]
        [RegularExpression(@"^(?:2|4|5|6|7|8)\d{7}$", ErrorMessage = "Teléfono inválido (CR: 8 dígitos inicia 2/4/5/6/7/8)")]
        public string Telefono { get; set; } = string.Empty;
    }
}