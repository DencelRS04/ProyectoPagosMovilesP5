using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PagosMoviles.API.DTOs;
using System.Text.RegularExpressions;

namespace PagosMoviles.API.Controllers;

[ApiController]
[Route("api/external-entity")]
[Tags("Mock Entidad Externa")]
[AllowAnonymous]
public class ExternalEntityMockController : ControllerBase
{
    // Costa Rica: 8 dígitos, inicia 2/4/5/6/7/8
    private static readonly Regex TelefonoCR = new(@"^(?:2|4|5|6|7|8)\d{7}$", RegexOptions.Compiled);

    [HttpPost("transactions/process")]
    public IActionResult ProcessExternalTransaction([FromBody] TransactionSendDto transaction)
    {
        if (transaction == null)
        {
            return BadRequest(new TransactionResponseDto
            {
                Codigo = 400,
                Descripcion = "Body requerido."
            });
        }

        // Validaciones específicas (sin mensajes genéricos)
        if (string.IsNullOrWhiteSpace(transaction.EntidadOrigen))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "EntidadOrigen es requerida." });

        if (string.IsNullOrWhiteSpace(transaction.TelefonoOrigen))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoOrigen es requerido." });

        if (transaction.TelefonoOrigen != transaction.TelefonoOrigen.Trim())
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoOrigen no puede tener espacios al inicio o al final." });

        if (!TelefonoCR.IsMatch(transaction.TelefonoOrigen))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoOrigen inválido. Debe ser CR: 8 dígitos e iniciar con 2,4,5,6,7 u 8." });

        if (string.IsNullOrWhiteSpace(transaction.NombreOrigen))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "NombreOrigen es requerido." });

        if (transaction.NombreOrigen != transaction.NombreOrigen.Trim())
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "NombreOrigen no puede tener espacios al inicio o al final." });

        if (string.IsNullOrWhiteSpace(transaction.TelefonoDestino))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoDestino es requerido." });

        if (transaction.TelefonoDestino != transaction.TelefonoDestino.Trim())
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoDestino no puede tener espacios al inicio o al final." });

        if (!TelefonoCR.IsMatch(transaction.TelefonoDestino))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "TelefonoDestino inválido. Debe ser CR: 8 dígitos e iniciar con 2,4,5,6,7 u 8." });

        if (transaction.Monto <= 0)
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "El monto debe ser mayor a 0." });

        if (transaction.Monto > 100000)
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "El monto no debe ser superior a 100.000." });

        if (string.IsNullOrWhiteSpace(transaction.Descripcion))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "Descripcion es requerida." });

        if (transaction.Descripcion != transaction.Descripcion.Trim())
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "Descripcion no puede tener espacios al inicio o al final." });

        if (transaction.Descripcion.Length > 25)
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "La descripción no puede superar 25 caracteres." });

        // Regla mock: rechaza destino que inicie con 9
        if (transaction.TelefonoDestino.StartsWith("9"))
            return BadRequest(new TransactionResponseDto { Codigo = 400, Descripcion = "Número de teléfono destino inválido en entidad externa." });

        return Ok(new TransactionResponseDto
        {
            Codigo = 200,
            Descripcion = $"Transacción procesada exitosamente. {transaction.Monto} enviado a {transaction.TelefonoDestino}"
        });
    }
}