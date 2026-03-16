using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.PortalWeb.Models.Auth
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; }

        public string MensajeError { get; set; }
        public bool Bloqueado { get; set; }

        public LoginInputModel()
        {
            Usuario = string.Empty;
            Contrasena = string.Empty;
            MensajeError = string.Empty;
        }
    }
}