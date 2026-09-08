using Microsoft.AspNetCore.Http;
using ProjektPWSW.Api.Dtos;
using Mindee;
using Mindee.Input;
using Mindee.Parsing.V2;
using System.Text.Json;
using System.Globalization;

namespace ProjektPWSW.Api.Services
{
    public class ReceiptOcrService
    {
        private const string ApiKey = "md_588W4TmQSPeFN0g1vCQYaiZEK82JCvj2td6KAnwi2QM"; // 🔴 WSTAW SWÓJ KLUCZ
        private const string ModelId = "d659d86f-6530-45a7-98de-7c046902077b";

        public async Task<List<ParsedExpenseDto>> ParseReceiptAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Plik jest pusty");

            // ✅ ZACHOWUJEMY ORYGINALNE ROZSZERZENIE
            var extension = Path.GetExtension(file.FileName);
            var tempFilePath = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}{extension}"
            );

            await using (var fs = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            try
            {
                var mindeeClient = new MindeeClientV2(ApiKey);

                var inferenceParams = new InferenceParameters(
                    modelId: ModelId,
                    rag: true,
                    rawText: false
                );

                var inputSource = new LocalInputSource(tempFilePath);

                var response = await mindeeClient.EnqueueAndGetInferenceAsync(
                    inputSource,
                    inferenceParams
                );

                // 🔥 surowy JSON – JEDYNE ŹRÓDŁO PRAWDY w 3.39.0
                return ParseFromRawJson(response.RawResponse);
            }
            finally
            {
                if (File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
            }
        }

        private static List<ParsedExpenseDto> ParseFromRawJson(string rawJson)
        {
            var result = new List<ParsedExpenseDto>();

            using var doc = JsonDocument.Parse(rawJson);

            if (!doc.RootElement
                .GetProperty("inference")
                .GetProperty("result")
                .GetProperty("fields")
                .TryGetProperty("line_items", out var lineItems))
            {
                // fallback: pojedyncze pola
                return ParseSingleFields(doc, result);
            }

            // ✅ PRZYPADEK: line_items jest OBIEKTEM
            if (lineItems.ValueKind == JsonValueKind.Object)
            {
                ParseLineItemObject(lineItems, result);
                return result;
            }

            // ✅ PRZYPADEK: line_items jest TABLICĄ
            if (lineItems.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in lineItems.EnumerateArray())
                {
                    ParseLineItemObject(item, result);
                }
            }

            return result;
        }

        private static void ParseLineItemObject(JsonElement item, List<ParsedExpenseDto> result)
        {
            string description = item.TryGetProperty("description", out var desc)
                ? desc.GetProperty("value").GetString()
                : null;

            string amountStr = item.TryGetProperty("amount", out var amt)
                ? amt.GetProperty("value").GetString()
                : null;

            decimal.TryParse(
                amountStr?.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal amount
            );

            result.Add(new ParsedExpenseDto
            {
                Description = description,
                Amount = amount,
                CategoryId = null
            });
        }

        private static List<ParsedExpenseDto> ParseSingleFields(
            JsonDocument doc,
            List<ParsedExpenseDto> result)
        {
            var fields = doc.RootElement
                .GetProperty("inference")
                .GetProperty("result")
                .GetProperty("fields");

            if (!fields.TryGetProperty("description", out var desc) ||
                !fields.TryGetProperty("amount", out var amt))
                return result;

            decimal.TryParse(
                amt.GetProperty("value").GetString()?.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal amount
            );

            result.Add(new ParsedExpenseDto
            {
                Description = desc.GetProperty("value").GetString(),
                Amount = amount,
                CategoryId = null
            });

            return result;
        }
    }
}