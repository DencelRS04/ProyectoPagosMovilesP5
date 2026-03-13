namespace PagosMoviles.API.Models;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TipoIdentificacion { get; set; } = string.Empty;
    public string Identificacion { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}
