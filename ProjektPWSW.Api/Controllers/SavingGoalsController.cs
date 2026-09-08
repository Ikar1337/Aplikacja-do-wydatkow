using Microsoft.AspNetCore.Mvc;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Services;

namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/savinggoals")]
public class SavingGoalsController : ControllerBase
{
    private readonly SavingGoalService _service;

    public SavingGoalsController(SavingGoalService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateSavingGoalDto dto)
        => Ok(await _service.AddAsync(dto));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateSavingGoalDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/add")]
    public async Task<IActionResult> AddAmount(int id, [FromQuery] decimal amount)
        => Ok(await _service.AddAmountAsync(id, amount));
}
