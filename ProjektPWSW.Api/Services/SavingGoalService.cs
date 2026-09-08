using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Models;

namespace ProjektPWSW.Api.Services;

public class SavingGoalService
{
    private readonly AppDbContext _db;

    public SavingGoalService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SavingGoalDto>> GetAllAsync()
    {
        var goals = await _db.SavingGoals.OrderBy(g => g.Id).ToListAsync();
        return goals.Select(ToDto).ToList();
    }

    public async Task<SavingGoalDto> AddAsync(CreateSavingGoalDto dto)
    {
        Validate(dto.Name, dto.TargetAmount, dto.CurrentAmount);

        var goal = new SavingGoal
        {
            Name = dto.Name.Trim(),
            TargetAmount = dto.TargetAmount,
            TargetDate = dto.TargetDate,
            CurrentAmount = dto.CurrentAmount
        };

        _db.SavingGoals.Add(goal);
        await _db.SaveChangesAsync();

        return ToDto(goal);
    }

    public async Task UpdateAsync(int id, UpdateSavingGoalDto dto)
    {
        Validate(dto.Name, dto.TargetAmount, dto.CurrentAmount);

        var goal = await _db.SavingGoals.FindAsync(id);
        if (goal is null) throw new Exception("Cel oszczędnościowy nie istnieje");

        goal.Name = dto.Name.Trim();
        goal.TargetAmount = dto.TargetAmount;
        goal.TargetDate = dto.TargetDate;
        goal.CurrentAmount = dto.CurrentAmount;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var goal = await _db.SavingGoals.FindAsync(id);
        if (goal is null) throw new Exception("Cel oszczędnościowy nie istnieje");

        _db.SavingGoals.Remove(goal);
        await _db.SaveChangesAsync();
    }
    public async Task<SavingGoalDto> AddAmountAsync(int id, decimal amount)
    {
        if (amount <= 0) throw new ArgumentException("Kwota dopłaty musi być > 0");

        var goal = await _db.SavingGoals.FindAsync(id);
        if (goal is null) throw new Exception("Cel oszczędnościowy nie istnieje");

        goal.CurrentAmount += amount;
        await _db.SaveChangesAsync();

        return ToDto(goal);
    }

    private static void Validate(string name, decimal targetAmount, decimal currentAmount)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nazwa celu nie może być pusta");

        if (targetAmount <= 0)
            throw new ArgumentException("Kwota docelowa musi być większa od 0");

        if (currentAmount < 0)
            throw new ArgumentException("Aktualna kwota nie może być ujemna");

        if (currentAmount > targetAmount)
            throw new ArgumentException("Aktualna kwota nie może przekraczać kwoty docelowej");
    }

    private static SavingGoalDto ToDto(SavingGoal g)
    {
        var percent = g.TargetAmount == 0 ? 0 : (g.CurrentAmount / g.TargetAmount) * 100m;
        if (percent > 100m) percent = 100m;

        var remaining = g.TargetAmount - g.CurrentAmount;
        if (remaining < 0) remaining = 0;

        return new SavingGoalDto
        {
            Id = g.Id,
            Name = g.Name,
            TargetAmount = g.TargetAmount,
            TargetDate = g.TargetDate,
            CurrentAmount = g.CurrentAmount,
            ProgressPercent = Math.Round(percent, 2),
            RemainingAmount = remaining
        };
    }
}
