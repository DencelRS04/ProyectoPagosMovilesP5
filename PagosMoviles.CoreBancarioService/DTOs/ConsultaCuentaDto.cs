using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.CoreBancarioService.DTOs
{
    public class ConsultaCuentaDto
    {
        [Required(ErrorMessage = "Identificación requerida")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Identificación inválida (solo números, 9-12 dígitos)")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número de cuenta requerido")]
        [MinLength(6)]
        [MaxLength(30)]
        public string NumeroCuenta { get; set; } = string.Empty;
    }
}
