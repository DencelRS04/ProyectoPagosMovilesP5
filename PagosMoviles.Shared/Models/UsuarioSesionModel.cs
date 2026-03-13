using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.Models
{

    public class UsuarioSesionModel
    {
        public string UsuarioId { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
    }
}
