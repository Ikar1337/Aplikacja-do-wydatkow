namespace ProjektPWSW.Api.Dtos;

public class SavingGoalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }

    public decimal ProgressPercent { get; set; }
    public decimal RemainingAmount { get; set; }
}
