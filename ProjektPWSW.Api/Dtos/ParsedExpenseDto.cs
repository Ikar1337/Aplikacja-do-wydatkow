namespace ProjektPWSW.Api.Dtos;

public class ParsedExpenseDto 
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
}
