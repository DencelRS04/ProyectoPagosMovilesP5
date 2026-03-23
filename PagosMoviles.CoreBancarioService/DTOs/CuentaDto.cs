using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.CoreBancarioService.DTOs
{
    public class CrearCuentaDto
    {
        [Required]
        public int ClienteId { get; set; }

        [Required]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required]
        public decimal Saldo { get; set; }
    }

    public class EditarCuentaDto
    {
        [Required]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required]
        public decimal Saldo { get; set; }
    }
}