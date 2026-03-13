using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PagosMoviles.UsuariosService.Models
{
    [Table("PagoMovil", Schema = "dbo")]
    public class PagoMovil
    {
        [Key]
        public int PagoMovilId { get; set; }

        public string Identificacion { get; set; } = string.Empty;

        public string NumeroCuenta { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public bool Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}
