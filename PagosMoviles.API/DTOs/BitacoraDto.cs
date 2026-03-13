namespace PagosMoviles.API.DTOs
{
    public class BitacoraDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Accion { get; set; } = string.Empty;
        public object? Anterior { get; set; }
        public object? Actual { get; set; }
    }
}