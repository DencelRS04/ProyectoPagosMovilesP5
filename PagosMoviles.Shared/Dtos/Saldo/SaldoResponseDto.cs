using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagosMoviles.Shared.DTOs.Saldo
{
    public class SaldoResponseDto
    {
        public AfiliacionDto Afiliacion { get; set; } = new AfiliacionDto();
        public decimal Saldo { get; set; }
    }

    public class AfiliacionDto
    {
        public string Identificacion { get; set; } = string.Empty;
        public string NumeroCuenta { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}

