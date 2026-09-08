namespace ProjektPWSW.Wpf.Dtos;

public class UpdateSavingGoalDto
{
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }
}
