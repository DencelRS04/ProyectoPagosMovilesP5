namespace PagosMoviles.PortalWeb.Models.Desinscripcion
{
    public class DesinscripcionFormModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La identificación es obligatoria.")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^\d{9,12}$",
            ErrorMessage = "Solo números, entre 9 y 12 dígitos.")]
        public string Identificacion { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El número de cuenta es obligatorio.")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[A-Za-z0-9]{3,30}$",
            ErrorMessage = "Solo letras y números, entre 3 y 30 caracteres.")]
        public string NumeroCuenta { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El teléfono es obligatorio.")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^(?:2|4|5|6|7|8)\d{7}$",
            ErrorMessage = "Teléfono CR inválido: 8 dígitos, inicia con 2, 4, 5, 6, 7 u 8.")]
        public string Telefono { get; set; } = string.Empty;
    }
}