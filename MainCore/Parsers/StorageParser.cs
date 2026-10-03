using System.Net;
using System.Text.Json;

namespace MainCore.Parsers
{
    public static class StorageParser
    {
        public record struct RawStorageDto(string? WoodText, string? ClayText, string? IronText, string? CropText, string? FreeCropText, string? WarehouseText, string? GranaryText);

        public static async Task<StorageDto> GetStorage(IPage page)
        {
            // 1. Instantly pull all raw text payloads out of the browser DOM at the exact same moment
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const getRawText = (selector) => {
                    const el = document.querySelector(selector);
                    return el ? (el.innerText || el.textContent).trim() : '';
                };

                return {
                    WoodText: getRawText('#l1'),
                    ClayText: getRawText('#l2'),
                    IronText: getRawText('#l3'),
                    CropText: getRawText('#l4'),
                    FreeCropText: getRawText('#stockBarFreeCrop'),
                    WarehouseText: getRawText('#stockBar div.warehouse div.capacity div.value'),
                    GranaryText: getRawText('#stockBar div.granary div.capacity div.value')
                };
            }");

            var text = jsonResult.GetRawText();
            var raw = JsonSerializer.Deserialize<RawStorageDto>(text);

            static long ProcessValue(string? rawValue)
            {
                string decoded = WebUtility.HtmlDecode(rawValue ?? "");
                return decoded.ParseLong();
            }

            return new StorageDto
            {
                Wood = ProcessValue(raw.WoodText),
                Clay = ProcessValue(raw.ClayText),
                Iron = ProcessValue(raw.IronText),
                Crop = ProcessValue(raw.CropText),
                FreeCrop = ProcessValue(raw.FreeCropText),
                Warehouse = ProcessValue(raw.WarehouseText),
                Granary = ProcessValue(raw.GranaryText)
            };
        }
    }
}