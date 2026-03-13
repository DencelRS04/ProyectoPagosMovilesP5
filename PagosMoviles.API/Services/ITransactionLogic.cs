using PagosMoviles.API.DTOs;

namespace PagosMoviles.API.Services;

/// <summary>
/// Interfaz para la lógica de negocio de transacciones
/// </summary>
public interface ITransactionLogic
{
    /// SRV7: Recibe y procesa una transacción
    /// Endpoint: POST /api/transactions/process
    Task<BusinessLogicResponseDto> ProcessTransaction(TransactionRequestDto transaction);
    /// SRV12: Resuelve la ruta de una transacción (interna o externa)
    /// Endpoint: POST /api/transactions/route

    Task<BusinessLogicResponseDto> RouteTransaction(TransactionRouteDto transaction);
    /// SRV8: Envía una transacción a una entidad externa
    /// Endpoint: POST /api/transactions/send
    Task<BusinessLogicResponseDto> SendTransaction(TransactionSendDto transaction);
}
