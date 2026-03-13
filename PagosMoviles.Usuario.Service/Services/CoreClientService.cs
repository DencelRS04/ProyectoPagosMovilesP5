using PagosMoviles.UsuariosService.Utils;

namespace PagosMoviles.UsuariosService.Services
{
    public class CoreClientService
    {
        public ApiResponse<bool> VerificarClienteEnCore(string identificacion)
        {
            // MOCK TEMPORAL
            if (identificacion.StartsWith("3"))
            {
                return ApiResponse<bool>.Ok(true, "Cliente existe en el core.");
            }

            return ApiResponse<bool>.Fail("Cliente no encontrado en el core.");
        }
    }
}
