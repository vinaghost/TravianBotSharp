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

        public record struct HeroState(int Health, bool IsAtHome, bool IsAdventureReady);

        public static async Task<HeroState> GetHeroStateAsync(IPage page)
        {
            var heroStatus = page.Locator("div.heroStatus a");
            await heroStatus.WaitForAsync();

            var result = await page.EvaluateAsync<HeroState>(@"() => {
                let health = -1;
                const healthMask = document.getElementById('healthMask');
                if (healthMask) {
                    const path = healthMask.querySelector('path');
                    if (path) {
                        const dAttr = path.getAttribute('d');
                        if (dAttr) {
                            const commands = dAttr.trim().split(/\s+/);
                            if (commands.length >= 2) {
                                const xx = parseFloat(commands[commands.length - 2]);
                                const yy = parseFloat(commands[commands.length - 1]);

                                if (!isNaN(xx) && !isNaN(yy)) {
                                    const rad = Math.atan2(yy - 55, xx - 55);
                                    health = Math.round(-56.173 * rad + 96.077);
                                } else {
                                    health = 0;
                                }
                            }
                        }
                    }
                }

                const heroHomeElement = document.querySelector('div.heroStatus a i.heroHome');
                const isAtHome = heroHomeElement !== null;

                const adventureButton = document.querySelector('a.adventure.round.attention');
                const isAdventureReady = adventureButton !== null;

                return {
                    Health: health,
                    IsAtHome: isAtHome,
                    IsAdventureReady: isAdventureReady
                };
            }");

            return result;
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