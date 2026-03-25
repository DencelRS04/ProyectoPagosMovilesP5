namespace PagosMoviles.AdminWeb.Models.Usuarios
{
    public class UsuarioViewModel
    {
        public int UsuarioId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public int RolId { get; set; }
        public int IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
    }

    public class UsuarioFormModel
    {
        public int? UsuarioId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El nombre es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El email es obligatorio.")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La identificación es obligatoria.")]
        public string Identificacion { get; set; } = string.Empty;

        public string TipoIdentificacion { get; set; } = "CC";

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El teléfono es obligatorio.")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 dígitos.")]
        public string Telefono { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "La contraseña debe tener mínimo 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        public int RolId { get; set; } = 2;

        public int IntentosFallidos { get; set; }

        public bool Bloqueado { get; set; }
    }
}