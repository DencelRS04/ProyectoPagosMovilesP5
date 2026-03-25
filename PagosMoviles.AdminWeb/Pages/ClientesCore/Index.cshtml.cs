using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PagosMoviles.AdminWeb.Models.ClientesCore;
using PagosMoviles.AdminWeb.Services.ClientesCore;

namespace PagosMoviles.AdminWeb.Pages.ClientesCore
{
    public class IndexModel : PageModel
    {
        private readonly ClientesCoreService _service;

        public IndexModel(ClientesCoreService service)
        {
            _service = service;
        }

        public List<ClienteViewModel> Clientes { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Busqueda { get; set; }

        [BindProperty]
        public ClienteCreateModel Cliente { get; set; } = new();

        [BindProperty]
        public ClienteEditModel ClienteEditar { get; set; } = new();

        public async Task OnGetAsync()
        {
            Clientes = await _service.ListarAsync();

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                var b = Busqueda.ToLower();

                Clientes = Clientes
                    .Where(c =>
                        (!string.IsNullOrWhiteSpace(c.Identificacion) && c.Identificacion.ToLower().Contains(b)) ||
                        (!string.IsNullOrWhiteSpace(c.NombreCompleto) && c.NombreCompleto.ToLower().Contains(b)) ||
                        (!string.IsNullOrWhiteSpace(c.Telefono) && c.Telefono.ToLower().Contains(b)))
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            Cliente = LeerFormularioCrear();

            ModelState.Clear();
            if (!TryValidateModel(Cliente, nameof(Cliente)))
            {
                Clientes = await _service.ListarAsync();
                ViewData["AbrirModalCrear"] = true;
                return Page();
            }

            var model = new ClienteViewModel
            {
                Identificacion = Cliente.Identificacion,
                TipoIdentificacion = Cliente.TipoIdentificacion,
                NombreCompleto = Cliente.NombreCompleto,
                FechaNacimiento = Cliente.FechaNacimiento,
                Telefono = Cliente.Telefono,
                Activo = Cliente.Activo
            };

            var resultado = await _service.CrearAsync(model);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                ViewData["AbrirModalCrear"] = true;
                return Page();
            }

            TempData["MensajeExito"] = "Cliente creado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditarAsync(int id)
        {
            ClienteEditar = LeerFormularioEditar(id);

            ModelState.Clear();
            if (!TryValidateModel(ClienteEditar, nameof(ClienteEditar)))
            {
                Clientes = await _service.ListarAsync();
                ViewData["AbrirModalEditar"] = true;
                return Page();
            }

            var model = new ClienteViewModel
            {
                ClienteId = id,
                Identificacion = ClienteEditar.Identificacion,
                TipoIdentificacion = ClienteEditar.TipoIdentificacion,
                NombreCompleto = ClienteEditar.NombreCompleto,
                FechaNacimiento = ClienteEditar.FechaNacimiento,
                Telefono = ClienteEditar.Telefono,
                Activo = ClienteEditar.Activo
            };

            var resultado = await _service.ActualizarAsync(id, model);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                ViewData["AbrirModalEditar"] = true;
                return Page();
            }

            TempData["MensajeExito"] = "Cliente actualizado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            var resultado = await _service.EliminarAsync(id);

            if (!resultado.ok)
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                Clientes = await _service.ListarAsync();
                return Page();
            }

            TempData["MensajeExito"] = "Cliente desactivado correctamente.";
            return RedirectToPage();
        }

        private ClienteCreateModel LeerFormularioCrear()
        {
            return new ClienteCreateModel
            {
                Identificacion = ObtenerValor("Cliente.Identificacion", "Identificacion"),
                TipoIdentificacion = ObtenerValor("Cliente.TipoIdentificacion", "TipoIdentificacion"),
                NombreCompleto = ObtenerValor("Cliente.NombreCompleto", "NombreCompleto"),
                FechaNacimiento = ObtenerFecha("Cliente.FechaNacimiento", "FechaNacimiento"),
                Telefono = ObtenerValor("Cliente.Telefono", "Telefono"),
                Activo = ObtenerBool("Cliente.Activo", "Activo")
            };
        }

        private ClienteEditModel LeerFormularioEditar(int id)
        {
            return new ClienteEditModel
            {
                ClienteId = id,
                Identificacion = ObtenerValor("ClienteEditar.Identificacion", "Identificacion"),
                TipoIdentificacion = ObtenerValor("ClienteEditar.TipoIdentificacion", "TipoIdentificacion"),
                NombreCompleto = ObtenerValor("ClienteEditar.NombreCompleto", "NombreCompleto"),
                FechaNacimiento = ObtenerFecha("ClienteEditar.FechaNacimiento", "FechaNacimiento"),
                Telefono = ObtenerValor("ClienteEditar.Telefono", "Telefono"),
                Activo = ObtenerBool("ClienteEditar.Activo", "Activo")
            };
        }

        private string ObtenerValor(string conPrefijo, string sinPrefijo)
        {
            return Request.Form[conPrefijo].FirstOrDefault()
                ?? Request.Form[sinPrefijo].FirstOrDefault()
                ?? string.Empty;
        }

        private DateTime? ObtenerFecha(string conPrefijo, string sinPrefijo)
        {
            var valor = Request.Form[conPrefijo].FirstOrDefault()
                ?? Request.Form[sinPrefijo].FirstOrDefault();

            if (DateTime.TryParse(valor, out var fecha))
                return fecha;

            return null;
        }

        private bool ObtenerBool(string conPrefijo, string sinPrefijo)
        {
            var valor = Request.Form[conPrefijo].FirstOrDefault()
                ?? Request.Form[sinPrefijo].FirstOrDefault();

            return valor == "true" || valor == "on";
        }
    }
}