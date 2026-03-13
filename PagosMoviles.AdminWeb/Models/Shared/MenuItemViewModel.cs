namespace PagosMoviles.AdminWeb.Models.Shared
{
    public class MenuItemViewModel
    {
        public string Texto { get; set; } = string.Empty;
        public string Controlador { get; set; } = string.Empty;
        public string Accion { get; set; } = "Index";
    }
}
