using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs
{
    public class ApiResponse<T>
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public T Datos { get; set; }
    }
}
