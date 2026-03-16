namespace PagosMoviles.AdminWeb.Models.Entidades
{
    public class ApiResponse<T>
    {
        public bool Exito { get; set; }

        public string Descripcion { get; set; }

        public T Datos { get; set; }
    }
}