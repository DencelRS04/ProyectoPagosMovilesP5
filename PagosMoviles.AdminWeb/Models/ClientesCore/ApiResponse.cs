namespace PagosMoviles.AdminWeb.Models.ClientesCore
{
    public class ApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public T? Datos { get; set; }
    }
}