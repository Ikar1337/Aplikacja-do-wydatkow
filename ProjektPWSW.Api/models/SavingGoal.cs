namespace ProjektPWSW.Api.Models;

public class SavingGoal
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }
}
