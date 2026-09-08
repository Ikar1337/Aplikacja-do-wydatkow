using Microsoft.AspNetCore.Mvc;
using ProjektPWSW.Api.Services;

namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController : ControllerBase
{
    private readonly ReportService _service;

    public ReportController(ReportService service)
    {
        _service = service;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GenerateMonthly(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var content = await _service.GenerateMonthlyReportAsync(year, month);
        var path = await _service.SaveToFileAsync(
            content,
            $"Raport_{year}_{month:D2}.txt");

        return Ok(new
        {
            path,
            content
        });
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GenerateYearly([FromQuery] int year)
    {
        var content = await _service.GenerateYearlyReportAsync(year);
        var path = await _service.SaveToFileAsync(
            content,
            $"Raport_{year}.txt");

        return Ok(new
        {
            path,
            content
        });
    }

    [HttpGet("range")]
    public async Task<IActionResult> GenerateRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var content = await _service.GenerateRangeReportAsync(from, to);
        var path = await _service.SaveToFileAsync(
            content,
            $"Raport_{from:yyyy_MM_dd}-{to:yyyy_MM_dd}.txt");

        return Ok(new
        {
            path,
            content
        });
    }
}
