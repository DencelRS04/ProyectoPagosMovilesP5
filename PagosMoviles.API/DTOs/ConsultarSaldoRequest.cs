using System.ComponentModel.DataAnnotations;

namespace PagosMoviles.API.DTOs;

public class ConsultarMovimientosRequest
{
    [Required(ErrorMessage = "El teléfono es requerido.")]
    public string Telefono { get; set; } = "";

    [Required(ErrorMessage = "La identificación es requerida.")]
    public string Identificacion { get; set; } = "";
}