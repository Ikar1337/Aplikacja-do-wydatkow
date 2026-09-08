using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ProjektPWSW.Api.Dtos;
using ProjektPWSW.Api.Services;

namespace ProjektPWSW.Api.Controllers;

[ApiController]
[Route("api/expenses")] // Ustawiamy taki adres, bo tak wpisaliśmy w WPF
public class ReceiptController : ControllerBase
{
	private readonly ReceiptOcrService _ocrService;

	public ReceiptController(ReceiptOcrService ocrService)
	{
		_ocrService = ocrService;
	}

	[HttpPost("parse-receipt")]
	public async Task<IActionResult> ParseReceipt(IFormFile file)
	{
		if (file == null || file.Length == 0)
		{
			return BadRequest("Nie przesłano pliku lub plik jest pusty.");
		}

		var parsedItems = await _ocrService.ParseReceiptAsync(file);
		return Ok(parsedItems);
	}
}