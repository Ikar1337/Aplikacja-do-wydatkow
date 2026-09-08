namespace ProjektPWSW.Wpf.Dtos;

public class CreateSavingGoalDto
{
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }
}
