using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.CoreBancarioService.DTOs
{
    public class AplicarTransaccionDto
    {
        [Required(ErrorMessage = "Identificación requerida")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "Identificación inválida (solo números, 9-12 dígitos)")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Número de cuenta requerido")]
        [MinLength(6)]
        [MaxLength(30)]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo requerido")]
        [RegularExpression("^(C|D|CREDITO|DEBITO)$", ErrorMessage = "Tipo debe ser C, D, CREDITO o DEBITO")]
        public string Tipo { get; set; } = string.Empty;

        [Range(0.01, 999999999, ErrorMessage = "Monto inválido")]
        public decimal Monto { get; set; }
    }
}
