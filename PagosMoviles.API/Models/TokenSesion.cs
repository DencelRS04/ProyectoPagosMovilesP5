namespace PagosMoviles.API.Models;

public class TokenSesion
{
    public int TokenId { get; set; }
    public int UsuarioId { get; set; }
    public string JwtToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime FechaExpiracion { get; set; } 

}
