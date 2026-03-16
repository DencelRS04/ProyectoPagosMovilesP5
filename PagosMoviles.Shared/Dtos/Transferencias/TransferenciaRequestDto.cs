using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs.Transferencias
{
    public class TransferenciaRequestDto
    {
        public string TelefonoOrigen { get; set; } = string.Empty;
        public string NombreOrigen { get; set; } = string.Empty;
        public string TelefonoDestino { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string EntidadDestino { get; set; } = string.Empty;
    }

    public class TransferenciaResponseDto
    {
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
