using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Models;

namespace ProjektPWSW.Api.Services;

public class SummaryService
{
    private readonly AppDbContext _db;

    public SummaryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int year, int month)
    {
        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .ToListAsync();

        var total = expenses.Sum(e => e.Amount);

        var categoryGroups = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key.Id,
                CategoryName = g.Key.Name,
                Amount = g.Sum(x => x.Amount)
            })
            .ToList();

        foreach (var c in categoryGroups)
        {
            c.Percentage = total == 0 ? 0 : Math.Round((c.Amount / total) * 100, 2);
        }

        return new MonthlySummaryDto
        {
            TotalAmount = total,
            TransactionCount = expenses.Count,
            Categories = categoryGroups
        };
    }
    public async Task<MonthlySummaryDto> GetYearlySummaryAsync(int year)
    {
        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Year == year)
            .ToListAsync();

        return BuildSummary(expenses);
    }

    public async Task<MonthlySummaryDto> GetRangeSummaryAsync(DateTime from, DateTime to)
    {
        var expenses = await _db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= from && e.Date <= to)
            .ToListAsync();

        return BuildSummary(expenses);
    }

    private MonthlySummaryDto BuildSummary(List<Expense> expenses)
    {
        var total = expenses.Sum(x => x.Amount);
        var count = expenses.Count;

        var byCat = expenses
            .GroupBy(x => new { x.CategoryId, x.Category!.Name })
            .Select(g => new CategorySummaryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Amount = g.Sum(x => x.Amount),
                Percentage = total == 0 ? 0 : Math.Round((g.Sum(x => x.Amount) / total) * 100, 2)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return new MonthlySummaryDto
        {
            TotalAmount = total,
            TransactionCount = count,
            Categories = byCat
        };
    }

}
