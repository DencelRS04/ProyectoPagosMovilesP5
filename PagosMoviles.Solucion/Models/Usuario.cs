using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PagosMoviles.UsuariosService.Models
{
    [Table("Usuario", Schema = "dbo")]
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        public string Identificacion { get; set; } = string.Empty;

        [Required]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public int RolId { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Allow Nulls = true en BD
        public DateTime FechaCreacion { get; set; }

        // int NOT NULL — ya existía en BD
        public int IntentosFallidos { get; set; }

        // bit NOT NULL — ya existía en BD
        public bool Bloqueado { get; set; }

        // Allow Nulls = true en BD
        public string? FotoPerfil { get; set; }

        // Allow Nulls = true en BD
        public string? ColorAvatar { get; set; }
    }
}
