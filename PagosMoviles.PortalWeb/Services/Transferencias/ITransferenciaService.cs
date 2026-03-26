using PagosMoviles.PortalWeb.Models.Transferencias;

namespace PagosMoviles.PortalWeb.Services.Transferencias
{
    public interface ITransferenciaService
    {
        Task<TransferenciaResponseDto> RealizarTransferencia(TransferenciaRequestDto dto);
    }
}