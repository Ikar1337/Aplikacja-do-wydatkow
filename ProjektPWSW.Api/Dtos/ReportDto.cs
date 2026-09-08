namespace ProjektPWSW.Api.Dtos;

public class ReportDto
{
    public string Title { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public string Content { get; set; } = null!;
}
