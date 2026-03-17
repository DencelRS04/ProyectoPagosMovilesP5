using PagosMoviles.Shared.DTOs.Saldo;

namespace PagosMoviles.PortalWeb.Services.Saldo
{
    public interface ISaldoService
    {
        Task<SaldoResponseDto> ConsultarSaldo(string telefono, string identificacion);
    }
}