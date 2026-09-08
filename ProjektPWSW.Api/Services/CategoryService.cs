using Microsoft.EntityFrameworkCore;
using ProjektPWSW.Api.Data;
using ProjektPWSW.Api.Models;

namespace ProjektPWSW.Api.Services;

public class CategoryService
{
    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _db.Categories.ToListAsync();
    }

    public async Task<Category> AddAsync(Category category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ArgumentException("Nazwa kategorii nie może być pusta");

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Category category)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Id == category.Id);
        if (!exists)
            throw new Exception("Kategoria nie istnieje");

        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var hasExpenses = await _db.Expenses.AnyAsync(e => e.CategoryId == id);
        if (hasExpenses)
            throw new Exception("Nie można usunąć kategorii, do której są przypisane wydatki");

        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            throw new Exception("Kategoria nie istnieje");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }
}
