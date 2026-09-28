using Microsoft.Playwright;
using System.Globalization;

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

            var heroHome = heroStatus.Locator("a i.heroHome");
            if (await heroHome.CountAsync() == 0) return false;

            var adventureButton = GetHeroAdventureButton(page);
            if (await adventureButton.CountAsync() == 0) return false;
            return true;
        }

        public record struct AdventureInfo(string Difficult, TimeSpan Duration, ILocator Button);

        public static async Task<List<AdventureInfo>> GetAdventureInfo(IPage page)
        {
            var rows = page.Locator("#heroAdventure tbody tr");
            int rowCount = await rows.CountAsync();

            var adventureInfoList = new List<AdventureInfo>();
            for (int i = 0; i < rowCount; i++)
            {
                var row = rows.Nth(i);

                string? difficultyClass = await row.Locator("td.difficulty i").GetAttributeAsync("class");
                string difficulty = !string.IsNullOrEmpty(difficultyClass)
                    ? difficultyClass.Replace("difficulty_", "")
                    : "unknown";

                string durationText = await row.Locator("td.duration .duration").InnerTextAsync();
                TimeSpan duration = TimeSpan.Parse(durationText, CultureInfo.InvariantCulture);

                ILocator buttonLocator = row.Locator("td.button button");
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