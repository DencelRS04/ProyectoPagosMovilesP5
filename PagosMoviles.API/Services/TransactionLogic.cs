using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PagosMoviles.API.Data;
using PagosMoviles.API.DTOs;
using PagosMoviles.API.Models;
using System.Text;

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
            var entityExists = await _context.EntidadesBancarias
                .AnyAsync(e => e.CodigoEntidad == transaction.EntidadOrigen);

            if (!entityExists)
            {
                await _bitacora.Registrar("SISTEMA", "SRV7: EntidadOrigen no registrada", transaction, null);
                return new BusinessLogicResponseDto
                {
                    StatusCode = 400,
                    Message = "Entidad origen no registrada"
                };
            }

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

            if (routeResponse.StatusCode == 200 || routeResponse.StatusCode == 201)
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
            return new BusinessLogicResponseDto
            {
                StatusCode = 500,
                Message = "Error interno del servidor"
            };
        }
    }

    public async Task<BusinessLogicResponseDto> RouteTransaction(TransactionRouteDto transaction)
    {
        try
        {
            var clienteOrigen = await _context.PagosMoviles
                .FirstOrDefaultAsync(p => p.Telefono == transaction.TelefonoOrigen && p.Estado);

            if (clienteOrigen == null)
            {
                await _bitacora.Registrar("SISTEMA", "SRV12: Origen no afiliado", transaction, null);
                return new BusinessLogicResponseDto
                {
                    StatusCode = 400,
                    Message = "Cliente origen no asociado a pagos móviles"
                };
            }

            var destinoInterno = await _context.PagosMoviles
                .FirstOrDefaultAsync(p => p.Telefono == transaction.TelefonoDestino && p.Estado);

            if (destinoInterno != null)
            {
                var internalResponse = await AplicarTransferenciaInternaEnCore(
                    clienteOrigen,
                    destinoInterno,
                    transaction.Monto);

                await _bitacora.Registrar("SISTEMA", "SRV12: Transacción interna", transaction, internalResponse);
                return internalResponse;
            }

            var entidadDestino = transaction.EntidadDestino;

            if (string.IsNullOrWhiteSpace(entidadDestino))
            {
                var first = _config.GetSection("ExternalEntities").GetChildren().FirstOrDefault();
                if (first == null)
                {
                    await _bitacora.Registrar("SISTEMA", "SRV12: No hay entidades externas configuradas", transaction, null);
                    return new BusinessLogicResponseDto
                    {
                        StatusCode = 404,
                        Message = "No hay entidades externas configuradas"
                    };
                }

                entidadDestino = first.Key;
            }

            var externalEntityUrl = _config[$"ExternalEntities:{entidadDestino}"];
            if (string.IsNullOrWhiteSpace(externalEntityUrl))
            {
                await _bitacora.Registrar("SISTEMA", "SRV12: Entidad destino no encontrada", transaction, null);
                return new BusinessLogicResponseDto
                {
                    StatusCode = 404,
                    Message = "Entidad destino no encontrada"
                };
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
            return new BusinessLogicResponseDto
            {
                StatusCode = 500,
                Message = "Error interno del servidor"
            };
        }
    }

    private async Task<BusinessLogicResponseDto> AplicarTransferenciaInternaEnCore(
        PagoMovil origen,
        PagoMovil destino,
        decimal monto)
    {
        try
        {
            var url = _config["CoreApi:ApplyTransactionUrl"];

            if (string.IsNullOrWhiteSpace(url))
            {
                return new BusinessLogicResponseDto
                {
                    StatusCode = 500,
                    Message = "No está configurada la URL del Core Bancario"
                };
            }

            var debitoRequest = new
            {
                identificacion = origen.Identificacion,
                numeroCuenta = origen.NumeroCuenta,
                tipo = "DEBITO",
                monto = monto
            };

            var debitoJson = JsonConvert.SerializeObject(debitoRequest);
            var debitoContent = new StringContent(debitoJson, Encoding.UTF8, "application/json");

            Console.WriteLine("URL Core:");
            Console.WriteLine(url);
            Console.WriteLine("JSON débito enviado al Core:");
            Console.WriteLine(debitoJson);

            var debitoResp = await _httpClient.PostAsync(url, debitoContent);
            var debitoBody = await debitoResp.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Core Débito: {(int)debitoResp.StatusCode}");
            Console.WriteLine($"Respuesta Core Débito: {debitoBody}");

            if (!debitoResp.IsSuccessStatusCode)
            {
                await _bitacora.Registrar("SISTEMA", $"SRV12 CORE DÉBITO ERROR: {debitoBody}", debitoRequest, null);

                return new BusinessLogicResponseDto
                {
                    StatusCode = (int)debitoResp.StatusCode,
                    Message = string.IsNullOrWhiteSpace(debitoBody)
                        ? "Error aplicando débito en core"
                        : debitoBody
                };
            }

            var creditoRequest = new
            {
                identificacion = destino.Identificacion,
                numeroCuenta = destino.NumeroCuenta,
                tipo = "CREDITO",
                monto = monto
            };

            var creditoJson = JsonConvert.SerializeObject(creditoRequest);
            var creditoContent = new StringContent(creditoJson, Encoding.UTF8, "application/json");

            Console.WriteLine("JSON crédito enviado al Core:");
            Console.WriteLine(creditoJson);

            var creditoResp = await _httpClient.PostAsync(url, creditoContent);
            var creditoBody = await creditoResp.Content.ReadAsStringAsync();

            Console.WriteLine($"Status Core Crédito: {(int)creditoResp.StatusCode}");
            Console.WriteLine($"Respuesta Core Crédito: {creditoBody}");

            if (!creditoResp.IsSuccessStatusCode)
            {
                await _bitacora.Registrar("SISTEMA", $"SRV12 CORE CRÉDITO ERROR: {creditoBody}", creditoRequest, null);

                return new BusinessLogicResponseDto
                {
                    StatusCode = (int)creditoResp.StatusCode,
                    Message = string.IsNullOrWhiteSpace(creditoBody)
                        ? "Débito aplicado, pero falló el crédito en core"
                        : creditoBody
                };
            }

            return new BusinessLogicResponseDto
            {
                StatusCode = 200,
                Message = "Transacción aplicada",
                ResponseObject = new TransactionResponseDto
                {
                    Codigo = 200,
                    Descripcion = "Transacción aplicada"
                }
            };
        }
        catch (Exception ex)
        {
            await _bitacora.Registrar("SISTEMA", $"SRV12 CORE ERROR: {ex.Message}", null, null);

            return new BusinessLogicResponseDto
            {
                StatusCode = 500,
                Message = $"Error aplicando transferencia en core: {ex.Message}"
            };
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
                return new BusinessLogicResponseDto
                {
                    StatusCode = 404,
                    Message = "Entidad destino no encontrada"
                };
            }

            var json = JsonConvert.SerializeObject(transaction);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{externalEntityUrl.TrimEnd('/')}/transactions/process";
            var resp = await _httpClient.PostAsync(url, httpContent);
            var body = await resp.Content.ReadAsStringAsync();

            TransactionResponseDto? dto = null;
            try { dto = JsonConvert.DeserializeObject<TransactionResponseDto>(body); } catch { }

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
                catch { }
            }

            dto ??= new TransactionResponseDto
            {
                Codigo = (int)resp.StatusCode,
                Descripcion = string.IsNullOrWhiteSpace(body) ? "Respuesta externa no válida" : body
            };

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
            return new BusinessLogicResponseDto
            {
                StatusCode = 500,
                Message = "Error interno del servidor"
            };
        }
    }
}