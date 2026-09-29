using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.Globalization;
using System.Text.Json;

namespace MainCore.Parsers
{
    public static class AdventureParser
    {
        public static async Task<TimeSpan> GetAdventureDuration(IPage page)
        {
            var timer = page.Locator("#heroAdventure span.timerReact");
            var text = await timer.InnerTextAsync();
            if (string.IsNullOrEmpty(text)) return TimeSpan.Zero;
            var duration = TimeSpan.Parse(text, CultureInfo.InvariantCulture);
            return duration;
        }

        public static ILocator GetAdventureDurationSection(IPage page)
        {
            var heroAdventure = page.Locator("#heroAdventure div.videoFeatureBonusBox.adventureDuration");
            return heroAdventure;
        }

        public static ILocator GetHeroAdventureButton(IPage page)
        {
            var adventureButton = page.Locator("a.adventure.round.attention");
            return adventureButton;
        }

        public static async Task<bool> CanStartAdventure(IPage page)
        {
            var heroStatus = page.Locator("div.heroStatus a");
            await heroStatus.WaitForAsync();

            var heroHome = heroStatus.Locator("i.heroHome");
            if (await heroHome.CountAsync() == 0) return false;

            var adventureButton = GetHeroAdventureButton(page);
            if (await adventureButton.CountAsync() == 0) return false;
            return true;
        }

        public record struct AdventureInfo(string Difficult, TimeSpan Duration, ILocator Button);
        public record struct RawAdventureDto(string? DifficultyClass, string? DurationText);

        public static async Task<List<AdventureInfo>> GetAdventureInfo(IPage page)
        {
            var jsonResult = await page.EvaluateAsync<JsonElement>(@"() => {
                const elements = document.querySelectorAll('#heroAdventure tbody tr');
                const result = [];

                elements.forEach(row => {
                    const diffEl = row.querySelector('td.difficulty i');
                    const durEl = row.querySelector('td.duration .duration');

                    result.push({
                        DifficultyClass: diffEl ? (diffEl.getAttribute('class') || '') : '',
                        DurationText: durEl ? (durEl.innerText || durEl.textContent).trim() : ''
                    });
                });
                return result;
            }");
            var text = jsonResult.GetRawText();
            var rawAdventures = JsonSerializer.Deserialize<List<RawAdventureDto>>(text) ?? throw new InvalidOperationException("Failed to deserialize adventure data from the page. Content: {text}");

            var rows = page.Locator("#heroAdventure tbody tr");

            var adventureInfoList = new List<AdventureInfo>();
            for (int i = 0; i < rawAdventures.Count; i++)
            {
                var raw = rawAdventures[i];

                string diffClass = raw.DifficultyClass ?? "";
                string difficulty = !string.IsNullOrEmpty(diffClass)
                    ? diffClass.Replace("difficulty_", "")
                    : "unknown";

                string durText = raw.DurationText ?? "00:00:00";
                TimeSpan duration = TimeSpan.Parse(durText, CultureInfo.InvariantCulture);

                ILocator buttonLocator = rows.Nth(i).Locator("td.button button");

                adventureInfoList.Add(new AdventureInfo(difficulty, duration, buttonLocator));
            }
            return adventureInfoList;
        }

        public static ILocator GetContinueButton(IPage page)
        {
            var continueButton = page.Locator("button.continue");
            return continueButton;
        }
    }
}