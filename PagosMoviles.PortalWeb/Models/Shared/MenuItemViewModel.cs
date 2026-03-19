namespace PagosMoviles.PortalWeb.Models.Shared
{
    public class MenuItemViewModel
    {
        public string Texto { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Icono { get; set; }
        public bool Activo { get; set; }
    }
}