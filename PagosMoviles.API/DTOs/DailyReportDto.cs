namespace PagosMoviles.API.DTOs;

public class DailyReportDto
{
    public string Date { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public List<TransaccionMovilDto> Transacciones { get; set; } = new();
}