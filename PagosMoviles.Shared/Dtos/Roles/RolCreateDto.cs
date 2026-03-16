using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs.Roles
{
    public class RolCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public List<int> Pantallas { get; set; } = new List<int>(); // ← agregar esta línea
    }
}