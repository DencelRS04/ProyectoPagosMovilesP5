using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PagosMoviles.CoreBancarioService.Models
{
    [Table("Cuenta")]
    public class Cuenta
    {
        [Key]
        public int CuentaId { get; set; }

        public int ClienteId { get; set; }

        public string NumeroCuenta { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; }

        [JsonIgnore]
        public Cliente? Cliente { get; set; }

        [JsonIgnore]
        public ICollection<Movimiento>? Movimientos { get; set; }
    }
}
