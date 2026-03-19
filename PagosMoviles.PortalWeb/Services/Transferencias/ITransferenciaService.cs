using PagosMoviles.Shared.DTOs.Transferencias;

namespace PagosMoviles.PortalWeb.Services.Transferencias
{
    public interface ITransferenciaService
    {
        Task<TransferenciaResponseDto> RealizarTransferencia(TransferenciaRequestDto dto);
    }
}