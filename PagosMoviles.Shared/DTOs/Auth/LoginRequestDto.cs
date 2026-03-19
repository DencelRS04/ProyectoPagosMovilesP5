using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs.Auth
{
    public class LoginRequestDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
