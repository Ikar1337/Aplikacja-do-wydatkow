using System.Text.Json.Serialization;

namespace ProjektPWSW.Api.Models;

public class Expense
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }

    [JsonIgnore]
    public Category Category { get; set; } = null!;
}
