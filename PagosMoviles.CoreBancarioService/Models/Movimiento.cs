using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PagosMoviles.CoreBancarioService.Models
{
    [Table("Movimiento")]
    public class Movimiento
    {
        [Key]
        public int MovimientoId { get; set; }

        public int CuentaId { get; set; }

        public string TipoMovimiento { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        [JsonIgnore]   // ✅ evita el ciclo
        public Cuenta? Cuenta { get; set; }
    }
}
