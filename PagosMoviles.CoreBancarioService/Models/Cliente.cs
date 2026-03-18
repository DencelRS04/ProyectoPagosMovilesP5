using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PagosMoviles.CoreBancarioService.Models
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string NombreCompleto { get; set; } = string.Empty;

        public DateTime FechaNacimiento { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public ICollection<Cuenta>? Cuentas { get; set; }
    }
}