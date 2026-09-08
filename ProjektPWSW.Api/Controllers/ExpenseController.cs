using Microsoft.AspNetCore.Mvc;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Models;
using ProjektPWSW.Api.Services;
namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/expenses")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseService _service;

    public ExpensesController(ExpenseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    => Ok(await _service.GetAllAsync());

    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to,
    [FromQuery] int? categoryId,
    [FromQuery] decimal? minAmount,
    [FromQuery] decimal? maxAmount,
    [FromQuery] string? sort)
    {
        return Ok(await _service.GetFilteredAsync(
            from, to, categoryId, minAmount, maxAmount, sort));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateExpenseDto dto)
    {
        var expense = new Expense
        {
            Date = dto.Date,
            Amount = dto.Amount,
            Description = dto.Description,
            CategoryId = dto.CategoryId
        };

        return Ok(await _service.AddAsync(expense));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateExpenseDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
