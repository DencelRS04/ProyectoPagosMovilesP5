using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs.Pantallas
{
    public class PantallaDto
    {
        public int PantallaId { get; set; }
        public string Identificador { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Ruta { get; set; }
    }
}

