namespace PagosMoviles.API.Models
{
    public class ApiError
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}
