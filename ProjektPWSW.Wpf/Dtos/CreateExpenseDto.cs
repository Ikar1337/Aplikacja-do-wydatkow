namespace ProjektPWSW.Wpf.Dtos;

public class CreateExpenseDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}
