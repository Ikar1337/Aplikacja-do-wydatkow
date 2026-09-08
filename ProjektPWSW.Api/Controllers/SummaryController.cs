using Microsoft.AspNetCore.Mvc;
using ProjektPWSW.Api.Services;

namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/summary")]
public class SummaryController : ControllerBase
{
    private readonly SummaryService _service;

    public SummaryController(SummaryService service)
    {
        _service = service;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthly(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        return Ok(await _service.GetMonthlySummaryAsync(year, month));
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> Yearly([FromQuery] int year)
    => Ok(await _service.GetYearlySummaryAsync(year));

    [HttpGet("range")]
    public async Task<IActionResult> Range([FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _service.GetRangeSummaryAsync(from, to));

}
