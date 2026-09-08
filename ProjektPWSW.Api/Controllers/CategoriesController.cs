using Microsoft.AspNetCore.Mvc;
using ProjektPWSW.Api.Models;
using ProjektPWSW.Api.Services;

namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoriesController(CategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
        => Ok(await _service.AddAsync(category));

    [HttpPut]
    public async Task<IActionResult> Update(Category category)
    {
        await _service.UpdateAsync(category);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
