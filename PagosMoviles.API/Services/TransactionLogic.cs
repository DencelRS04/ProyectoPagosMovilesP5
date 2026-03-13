using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Models;

namespace PagosMoviles.API.Services;

public class TransactionLogic : ITransactionLogic
{
    private readonly PagosMovilesDbContext _context;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly BitacoraService _bitacora;

    public TransactionLogic(
        PagosMovilesDbContext context,
        IConfiguration config,
        HttpClient httpClient,
        BitacoraService bitacora)
    {
        _context = context;
        _config = config;
        _httpClient = httpClient;
        _bitacora = bitacora;
    }

    public async Task<BusinessLogicResponseDto> ProcessTransaction(TransactionRequestDto transaction)
    {
        try
        {
            // Validar entidad origen registrada
            var entityExists = await _context.EntidadesBancarias
                .AnyAsync(e => e.CodigoEntidad == transaction.EntidadOrigen);

            if (!entityExists)
            {
                await _bitacora.Registrar("SISTEMA", "SRV7: EntidadOrigen no registrada", transaction, null);
                return new BusinessLogicResponseDto { StatusCode = 400, Message = "Entidad origen no registrada" };
            }

            // SRV12 (ruta)
            var routeRequest = new TransactionRouteDto
            {
                TelefonoOrigen = transaction.TelefonoOrigen,
                NombreOrigen = transaction.NombreOrigen,
                TelefonoDestino = transaction.TelefonoDestino,
                Monto = transaction.Monto,
                Descripcion = transaction.Descripcion,
                EntidadDestino = transaction.EntidadDestino
            };

            var routeResponse = await RouteTransaction(routeRequest);

            // Guardar si aplicada (interno o externo exitoso)
            if (routeResponse.StatusCode == 200)
            {
                var entity = new TransaccionMovil
                {
                    EntidadOrigen = transaction.EntidadOrigen,
                    EntidadDestino = transaction.EntidadDestino,
                    TelefonoOrigen = transaction.TelefonoOrigen,
                    TelefonoDestino = transaction.TelefonoDestino,
                    Monto = transaction.Monto,
                    Descripcion = transaction.Descripcion,
                    Fecha = DateTime.Now
                };

                _context.TransaccionesMoviles.Add(entity);
                await _context.SaveChangesAsync();
            }

            await _bitacora.Registrar("SISTEMA", "SRV7: Trama procesada", transaction, routeResponse);

            return routeResponse;
        }
        catch (Exception ex)
        {
            await _bitacora.Registrar("SISTEMA", $"SRV7 ERROR: {ex.Message}", transaction, null);
            return new BusinessLogicResponseDto { StatusCode = 500, Message = "Error interno del servidor" };
        }
    }

    public async Task<BusinessLogicResponseDto> RouteTransaction(TransactionRouteDto transaction)
    {
        try
        {
            // Validar cliente origen afiliado
            var clienteOrigen = await _context.PagosMoviles
                .FirstOrDefaultAsync(p => p.Telefono == transaction.TelefonoOrigen && p.Estado);

            if (clienteOrigen == null)
            {
                await _bitacora.Registrar("SISTEMA", "SRV12: Origen no afiliado", transaction, null);
                return new BusinessLogicResponseDto { StatusCode = 400, Message = "Cliente no asociado a pagos móviles" };
            }

            // Destino interno?
            var destinoInterno = await _context.PagosMoviles
                .FirstOrDefaultAsync(p => p.Telefono == transaction.TelefonoDestino && p.Estado);

            if (destinoInterno != null)
            {
                var ok = new BusinessLogicResponseDto
                {
                    StatusCode = 200,
                    Message = "Transacción aplicada",
                    ResponseObject = new TransactionResponseDto
                    {
                        Codigo = 200,
                        Descripcion = "Transacción aplicada"
                    }
                };

                await _bitacora.Registrar("SISTEMA", "SRV12: Transacción interna", transaction, ok);
                return ok;
            }

            // Externa: elegir entidad destino
            var entidadDestino = transaction.EntidadDestino;

            if (string.IsNullOrWhiteSpace(entidadDestino))
            {
                var first = _config.GetSection("ExternalEntities").GetChildren().FirstOrDefault();
                if (first == null)
                {
                    await _bitacora.Registrar("SISTEMA", "SRV12: No hay entidades externas configuradas", transaction, null);
                    return new BusinessLogicResponseDto { StatusCode = 404, Message = "No hay entidades externas configuradas" };
                }
                entidadDestino = first.Key;
            }

            var externalEntityUrl = _config[$"ExternalEntities:{entidadDestino}"];
            if (string.IsNullOrWhiteSpace(externalEntityUrl))
            {
                await _bitacora.Registrar("SISTEMA", "SRV12: Entidad destino no encontrada", transaction, null);
                return new BusinessLogicResponseDto { StatusCode = 404, Message = "Entidad destino no encontrada" };
            }

            var sendRequest = new TransactionSendDto
            {
                EntidadOrigen = entidadDestino,
                TelefonoOrigen = transaction.TelefonoOrigen,
                NombreOrigen = transaction.NombreOrigen,
                TelefonoDestino = transaction.TelefonoDestino,
                Monto = transaction.Monto,
                Descripcion = transaction.Descripcion
            };

            var externalResponse = await SendTransaction(sendRequest);
            await _bitacora.Registrar("SISTEMA", "SRV12: Transacción externa", sendRequest, externalResponse);

            return externalResponse;
        }
        catch (Exception ex)
        {
            await _bitacora.Registrar("SISTEMA", $"SRV12 ERROR: {ex.Message}", transaction, null);
            return new BusinessLogicResponseDto { StatusCode = 500, Message = "Error interno del servidor" };
        }
    }

    public async Task<BusinessLogicResponseDto> SendTransaction(TransactionSendDto transaction)
    {
        try
        {
            var externalEntityUrl = _config[$"ExternalEntities:{transaction.EntidadOrigen}"];
            if (string.IsNullOrWhiteSpace(externalEntityUrl))
            {
                await _bitacora.Registrar("SISTEMA", "SRV8: Entidad destino no encontrada", transaction, null);
                return new BusinessLogicResponseDto { StatusCode = 404, Message = "Entidad destino no encontrada" };
            }

            var json = JsonConvert.SerializeObject(transaction);
            var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var url = $"{externalEntityUrl.TrimEnd('/')}/transactions/process";
            var resp = await _httpClient.PostAsync(url, httpContent);
            var body = await resp.Content.ReadAsStringAsync();

            // 1) Intentar parsear como TransactionResponseDto (nuestro MOCK ya responde esto)
            TransactionResponseDto? dto = null;
            try { dto = JsonConvert.DeserializeObject<TransactionResponseDto>(body); } catch { /* ignore */ }

            // 2) Si viniera como ApiResponse, intentamos leer descripcion
            if (dto == null)
            {
                try
                {
                    var api = JsonConvert.DeserializeObject<ApiResponse>(body);
                    if (api != null)
                    {
                        dto = new TransactionResponseDto
                        {
                            Codigo = (int)resp.StatusCode,
                            Descripcion = api.descripcion ?? "Respuesta externa"
                        };
                    }
                }
                catch { /* ignore */ }
            }

            // 3) Fallback
            dto ??= new TransactionResponseDto
            {
                Codigo = (int)resp.StatusCode,
                Descripcion = string.IsNullOrWhiteSpace(body) ? "Respuesta externa no válida" : body
            };

            // Forzar a que el código sea el HTTP real
            dto.Codigo = (int)resp.StatusCode;

            var result = new BusinessLogicResponseDto
            {
                StatusCode = (int)resp.StatusCode,
                Message = dto.Descripcion,
                ResponseObject = dto
            };

            await _bitacora.Registrar("SISTEMA", "SRV8: Respuesta entidad externa", transaction, result);

            return result;
        }
        catch (Exception ex)
        {
            await _bitacora.Registrar("SISTEMA", $"SRV8 ERROR: {ex.Message}", transaction, null);
            return new BusinessLogicResponseDto { StatusCode = 500, Message = "Error interno del servidor" };
        }
    }
}