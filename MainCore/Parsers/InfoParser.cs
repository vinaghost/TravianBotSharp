using System.Text.Json;
using System.Text.RegularExpressions;

namespace MainCore.Parsers
{
    public static partial class InfoParser
    {
        public record struct RawAccountInfoDto(string? GoldText, string? SilverText, string? PlusClassAttr, string? TribeClassAttr);

        [GeneratedRegex(@"vid_(\d+)")]
        private static partial Regex TribeExtractor();

        public static async Task<AccountInfoDto> GetAccountInfo(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const goldEl = document.querySelector('div.ajaxReplaceableGoldAmount');
                const silverEl = document.querySelector('div.ajaxReplaceableSilverAmount');
                const editButton = document.querySelector('#sidebarBoxLinklist a.edit.round');
                const questmasterBtn = document.querySelector('#questmasterButton');

                return {
                    GoldText: goldEl ? (goldEl.innerText || goldEl.textContent).trim() : '0',
                    SilverText: silverEl ? (silverEl.innerText || silverEl.textContent).trim() : '0',
                    PlusClassAttr: editButton ? (editButton.getAttribute('class') || '') : '',
                    TribeClassAttr: questmasterBtn ? (questmasterBtn.getAttribute('class') || '') : ''
                };
            }");

            var raw = JsonSerializer.Deserialize<RawAccountInfoDto>(jsonResult.GetRawText());

            int gold = (raw.GoldText ?? "").ParseInt();
            int silver = (raw.SilverText ?? "").ParseInt();

            string plusAttr = raw.PlusClassAttr ?? "";
            bool hasPlus = plusAttr.Contains("green");

            string tribeSrc = raw.TribeClassAttr ?? "";
            var tribeMatch = TribeExtractor().Match(tribeSrc);
            TribeEnums tribe = TribeEnums.Any;

            if (tribeMatch.Success)
            {
                int tribeId = int.Parse(tribeMatch.Groups[1].Value);
                tribe = (TribeEnums)tribeId;
            }

            return new AccountInfoDto
            {
                Gold = gold,
                Silver = silver,
                HasPlusAccount = hasPlus,
                Tribe = tribe
            };
        }
    }
}