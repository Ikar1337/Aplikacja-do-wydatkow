namespace ProjektPWSW.Api.Dtos;

public class UpdateSavingGoalDto
{
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }
}
