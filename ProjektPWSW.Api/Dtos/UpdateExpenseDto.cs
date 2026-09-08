namespace ProjektPWSW.Api.Dtos;

public class UpdateExpenseDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}
