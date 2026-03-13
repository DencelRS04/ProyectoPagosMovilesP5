using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.API.DTOs
{
    public class ValidateTokenDto
    {
        [Required(ErrorMessage = "Debe enviar el token")]
        public string Token { get; set; } = string.Empty;
    }
}