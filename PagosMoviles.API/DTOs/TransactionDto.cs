using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PagosMoviles.API.DTOs;

public static class ValidationPatterns
{
    // Costa Rica: 8 dígitos numéricos, sin espacios/guiones, inicia en 2/4/5/6/7/8
    public const string TelefonoCR = @"^(?:2|4|5|6|7|8)\d{7}$";

    // No permitir espacios al inicio o final
    public const string NoEspaciosInicioFin = @"^\S(.*\S)?$";
}

/// <summary>
/// DTO para solicitud de transacción
/// </summary>
public class TransactionRequestDto
{
    [Required(ErrorMessage = "La entidad origen es requerida.")]
    [JsonPropertyName("entidadOrigen")]
    public string EntidadOrigen { get; set; } = "";

    [Required(ErrorMessage = "La entidad destino es requerida.")]
    [JsonPropertyName("entidadDestino")]
    public string EntidadDestino { get; set; } = "";

    [Required(ErrorMessage = "El teléfono origen es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono origen debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    [JsonPropertyName("telefonoOrigen")]
    public string TelefonoOrigen { get; set; } = "";

    [Required(ErrorMessage = "El nombre origen es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre origen no puede superar 100 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "El nombre origen no puede tener espacios al inicio o al final.")]
    [JsonPropertyName("nombreOrigen")]
    public string NombreOrigen { get; set; } = "";

    [Required(ErrorMessage = "El teléfono destino es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono destino debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    [JsonPropertyName("telefonoDestino")]
    public string TelefonoDestino { get; set; } = "";

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 100000, ErrorMessage = "El monto debe ser mayor a 0 y no debe superar 100.000.")]
    [JsonPropertyName("monto")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "La descripción no puede tener espacios al inicio o al final.")]
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = "";
}

/// <summary>
/// DTO para respuesta estándar de transacciones
/// </summary>
public class TransactionResponseDto
{
    // ✅ Usar código HTTP real (200/400/404/500)
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = "";
}

/// <summary>
/// DTO para resolución de transacciones (SRV12)
/// </summary>
public class TransactionRouteDto
{
    [Required(ErrorMessage = "El teléfono origen es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono origen debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    public string TelefonoOrigen { get; set; } = "";

    [Required(ErrorMessage = "El nombre origen es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre origen no puede superar 100 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "El nombre origen no puede tener espacios al inicio o al final.")]
    public string NombreOrigen { get; set; } = "";

    [Required(ErrorMessage = "El teléfono destino es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono destino debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    public string TelefonoDestino { get; set; } = "";

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 100000, ErrorMessage = "El monto debe ser mayor a 0 y no debe superar 100.000.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "La descripción no puede tener espacios al inicio o al final.")]
    public string Descripcion { get; set; } = "";

    public string? EntidadDestino { get; set; }
}

/// <summary>
/// DTO para envío de transacciones a entidades externas (SRV8)
/// </summary>
public class TransactionSendDto
{
    [Required(ErrorMessage = "La entidad origen es requerida.")]
    public string EntidadOrigen { get; set; } = "";

    [Required(ErrorMessage = "El teléfono origen es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono origen debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    public string TelefonoOrigen { get; set; } = "";

    [Required(ErrorMessage = "El nombre origen es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre origen no puede superar 100 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "El nombre origen no puede tener espacios al inicio o al final.")]
    public string NombreOrigen { get; set; } = "";

    [Required(ErrorMessage = "El teléfono destino es requerido.")]
    [RegularExpression(ValidationPatterns.TelefonoCR,
        ErrorMessage = "El teléfono destino debe tener 8 dígitos numéricos válidos para Costa Rica (sin espacios ni guiones).")]
    public string TelefonoDestino { get; set; } = "";

    [Required(ErrorMessage = "El monto es requerido.")]
    [Range(0.01, 100000, ErrorMessage = "El monto debe ser mayor a 0 y no debe superar 100.000.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La descripción es requerida.")]
    [StringLength(25, ErrorMessage = "La descripción no puede superar 25 caracteres.")]
    [RegularExpression(ValidationPatterns.NoEspaciosInicioFin, ErrorMessage = "La descripción no puede tener espacios al inicio o al final.")]
    public string Descripcion { get; set; } = "";
}

/// <summary>
/// DTO para respuesta de error estándar (si lo sigues usando)
/// </summary>
public class ErrorResponseDto
{
    // ✅ Código HTTP real (200/400/404/500)
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = "";
}

public static class TransactionEndpoints
{
    public const string BaseUrl = "http://localhost:7143/api/external-entity";

    public static readonly IDictionary<string, string> ExternalEntities = new Dictionary<string, string>
    {
        {"OTRO_BANCO001", "http://localhost:7143/api/external-entity"},
        {"BANCO_EXTERNO", "http://external-bank.com/api/transactions"},
        {"TRANSFERENCIAS_SA", "http://another-bank.com/api/process"}
    };
}