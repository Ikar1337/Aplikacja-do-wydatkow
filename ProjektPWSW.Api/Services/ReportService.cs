using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Models;
using System.Text;

namespace ProjektPWSW.Api.Services;

public class ReportService
{
    private readonly AppDbContext _db;
    private readonly SummaryService _summary;

    public ReportService(AppDbContext db, SummaryService summary)
    {
        _db = db;
        _summary = summary;
    }

    public async Task<string> GenerateMonthlyReportAsync(int year, int month)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"RAPORT MIESIĘCZNY {month:D2}/{year}");
        sb.AppendLine($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 40));

        var summary = await _summary.GetMonthlySummaryAsync(year, month);
        AppendSummarySection(sb, summary);

        sb.AppendLine();
        sb.AppendLine("Lista wydatków:");

        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .OrderBy(e => e.Date)
            .ToListAsync();

        AppendExpensesSection(sb, expenses);

        await AppendGoalsSectionAsync(sb);

        return sb.ToString();
    }

    public async Task<string> GenerateYearlyReportAsync(int year)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"RAPORT ROCZNY {year}");
        sb.AppendLine($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 40));

        var summary = await _summary.GetYearlySummaryAsync(year);
        AppendSummarySection(sb, summary);

        sb.AppendLine();
        sb.AppendLine("Lista wydatków:");

        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Year == year)
            .OrderBy(e => e.Date)
            .ToListAsync();

        AppendExpensesSection(sb, expenses);

        await AppendGoalsSectionAsync(sb);

        return sb.ToString();
    }

    public async Task<string> GenerateRangeReportAsync(DateTime from, DateTime to)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"RAPORT OKRESOWY {from:yyyy-MM-dd} - {to:yyyy-MM-dd}");
        sb.AppendLine($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 40));

        var summary = await _summary.GetRangeSummaryAsync(from, to);
        AppendSummarySection(sb, summary);

        sb.AppendLine();
        sb.AppendLine("Lista wydatków:");

        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)
            .ToListAsync();

        AppendExpensesSection(sb, expenses);

        await AppendGoalsSectionAsync(sb);

        return sb.ToString();
    }

    public async Task<string> SaveToFileAsync(string content, string fileName)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            fileName
        );

        await File.WriteAllTextAsync(path, content, Encoding.UTF8);
        return path;
    }

    private static void AppendSummarySection(StringBuilder sb, dynamic summary)
    {
        sb.AppendLine($"Suma wydatków: {summary.TotalAmount:0.00} zł");
        sb.AppendLine($"Liczba transakcji: {summary.TransactionCount}");
        sb.AppendLine();

        sb.AppendLine("Rozbicie na kategorie:");
        foreach (var c in summary.Categories)
        {
            sb.AppendLine($"- {c.CategoryName}: {c.Amount:0.00} zł ({c.Percentage:0.##}%)");
        }
    }

    private static void AppendExpensesSection(StringBuilder sb, List<Expense> expenses)

    {
        if (expenses.Count == 0)
        {
            sb.AppendLine("Brak wydatków w tym okresie.");
            return;
        }

        foreach (var e in expenses)
        {
            var cat = e.Category?.Name ?? "(brak kategorii)";
            var desc = string.IsNullOrWhiteSpace((string?)e.Description) ? "-" : (string)e.Description;
            sb.AppendLine($"{e.Date:yyyy-MM-dd} | {cat} | {e.Amount:0.00} zł | {desc}");
        }
    }

    private async Task AppendGoalsSectionAsync(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine(new string('-', 40));
        sb.AppendLine("CELE OSZCZĘDNOŚCIOWE (stan na dziś):");

        var goals = await _db.SavingGoals
            .OrderBy(g => g.TargetDate)
            .ToListAsync();

        if (goals.Count == 0)
        {
            sb.AppendLine("Brak celów.");
            return;
        }

        foreach (var g in goals)
        {
            var progress = g.TargetAmount == 0
                ? 0
                : Math.Round((g.CurrentAmount / g.TargetAmount) * 100m, 2);

            var remaining = g.TargetAmount - g.CurrentAmount;

            sb.AppendLine(
                $"- {g.Name}: {g.CurrentAmount:0.00}/{g.TargetAmount:0.00} zł " +
                $"({progress:0.##}%)  Brakuje: {remaining:0.00} zł  Termin: {g.TargetDate:yyyy-MM-dd}"
            );
        }
    }
}
