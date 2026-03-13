namespace PagosMoviles.API.DTOs
{
    public class RolDTO
    {
        public int? RolId { get; set; } 
        public string Nombre { get; set; } = string.Empty;
        public List<int> Pantallas { get; set; } = new();
    }
}
