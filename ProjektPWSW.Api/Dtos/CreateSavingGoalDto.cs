namespace ProjektPWSW.Api.Dtos;

public class CreateSavingGoalDto
{
    public string Name { get; set; } = null!;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; } 
}
