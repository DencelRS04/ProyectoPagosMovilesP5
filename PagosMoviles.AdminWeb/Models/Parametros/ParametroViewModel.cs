namespace PagosMoviles.AdminWeb.Models.Parametros
{
    public class ParametroViewModel
    {
        public string ParametroId { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class ParametroFormModel
    {
        // Guarda el ID original al editar (vacío = es nuevo)
        public string? ParametroIdOriginal { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El identificador es obligatorio.")]
        [System.ComponentModel.DataAnnotations.RegularExpression(@"^[A-Z]{1,10}$",
            ErrorMessage = "Solo letras mayúsculas, entre 1 y 10 caracteres.")]
        public string ParametroId { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "El valor es obligatorio.")]
        [System.ComponentModel.DataAnnotations.MaxLength(500,
            ErrorMessage = "Máximo 500 caracteres.")]
        public string Valor { get; set; } = string.Empty;
    }
}