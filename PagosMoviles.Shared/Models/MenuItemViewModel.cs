namespace PagosMoviles.Shared.Models
{
    public class MenuItemViewModel
    {
        public string Texto { get; set; }
        public string Url { get; set; }

        public MenuItemViewModel()
        {
            Texto = string.Empty;
            Url = string.Empty;
        }
    }
}