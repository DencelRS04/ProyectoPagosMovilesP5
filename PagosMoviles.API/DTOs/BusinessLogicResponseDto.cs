namespace PagosMoviles.API.DTOs;

/// <summary>
/// DTO para respuesta estándar de la lógica de negocio
/// </summary>
public class BusinessLogicResponseDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = "";
    public object? ResponseObject { get; set; }
}