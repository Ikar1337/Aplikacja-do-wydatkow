using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Models;

namespace ProjektPWSW.Api.Services;

public class ExpenseService
{
    private readonly AppDbContext _db;

    public ExpenseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Expense> AddAsync(Expense expense)
    {

        if (expense.Amount <= 0)
            throw new ArgumentException("Kwota musi być większa od 0");

        if (expense.Date == default)
            throw new ArgumentException("Data wydatku jest wymagana");

        var categoryExists = await _db.Categories
            .AnyAsync(c => c.Id == expense.CategoryId);

        if (!categoryExists)
            throw new ArgumentException("Wybrana kategoria nie istnieje");

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();
        return expense;
    }

    public async Task UpdateAsync(int id, UpdateExpenseDto dto)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense is null) throw new Exception("Wydatek nie istnieje");

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
        if (!categoryExists) throw new Exception("Kategoria nie istnieje");

        expense.Date = dto.Date;
        expense.Amount = dto.Amount;
        expense.Description = dto.Description;
        expense.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();
    }


    public async Task<List<ExpenseListDto>> GetAllAsync()
    {
        return await _db.Expenses
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseListDto
            {
                Id = e.Id,
                Date = e.Date,
                Amount = e.Amount,
                Description = e.Description,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name
            })
            .ToListAsync();
    }


    public async Task<List<ExpenseListDto>> GetFilteredAsync(
    DateTime? from,
    DateTime? to,
    int? categoryId,
    decimal? minAmount,
    decimal? maxAmount,
    string? sort)
    {
        var query = _db.Expenses
            .Include(e => e.Category)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Date <= to.Value);

        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);

        if (minAmount.HasValue)
            query = query.Where(e => e.Amount >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(e => e.Amount <= maxAmount.Value);

        query = sort switch
        {
            "date_asc" => query.OrderBy(e => e.Date),
            "date_desc" => query.OrderByDescending(e => e.Date),
            "amount_asc" => query.OrderBy(e => e.Amount),
            "amount_desc" => query.OrderByDescending(e => e.Amount),
            _ => query.OrderByDescending(e => e.Date)
        };

        return await query
            .Select(e => new ExpenseListDto
            {
                Id = e.Id,
                Date = e.Date,
                Amount = e.Amount,
                Description = e.Description,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name
            })
            .ToListAsync();
    }
    public async Task DeleteAsync(int id)
    {
        var expense = await _db.Expenses.FindAsync(id);
        if (expense == null)
            throw new Exception("Wydatek nie istnieje");

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
    }

}
