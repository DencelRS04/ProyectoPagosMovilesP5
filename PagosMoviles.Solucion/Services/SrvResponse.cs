namespace PagosMoviles.UsuariosService.Utils
{
    public class SrvResponse<T>
    {
        public int Codigo { get; set; }          // 0 ok, -1 error
        public string Descripcion { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object Datos { get; internal set; }

        public static SrvResponse<T> Ok(T? data, string descripcion)
            => new() { Codigo = 0, Descripcion = descripcion, Data = data };

        public static SrvResponse<T> Fail(string descripcion)
            => new() { Codigo = -1, Descripcion = descripcion, Data = default };
    }
}