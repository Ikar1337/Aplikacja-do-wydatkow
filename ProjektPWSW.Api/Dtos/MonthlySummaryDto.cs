namespace ProjektPWSW.Api.Dtos;

public class MonthlySummaryDto
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public List<CategorySummaryDto> Categories { get; set; } = new();
}

public class CategorySummaryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
