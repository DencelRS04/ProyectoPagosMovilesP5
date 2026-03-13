using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.AdminWeb.Models.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        public string? MensajeError { get; set; }
    }
}
