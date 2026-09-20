using Microsoft.Playwright;
using System.Globalization;

namespace MainCore.Parsers
{
    public static class AdventureParser
    {
        public static async Task<TimeSpan> GetAdventureDuration(IPage page)
        {
            var timer = page.Locator("#heroAdventure span.timer").First;
            var seconds = await timer.GetAttributeAsync("value");
            if (string.IsNullOrEmpty(seconds)) return TimeSpan.Zero;
            return TimeSpan.FromSeconds(double.Parse(seconds));
        }

        public static async Task<bool> IsAdventurePage(IPage page)
        {
            var heroAdventure = page.Locator("#heroAdventure").First;
            var isVisible = await heroAdventure.IsVisibleAsync();
            return isVisible;
        }

        public static ILocator GetHeroAdventureButton(IPage page)
        {
            var adventureButton = page.Locator("a.adventure.round");
            return adventureButton;
        }

        public static async Task<bool> CanStartAdventure(IPage page)
        {
            var heroHome = page.Locator("div.heroStatus i.heroHome").First;
            var isHeroHomeVisible = await heroHome.IsVisibleAsync();
            if (!isHeroHomeVisible) return false;

            var adventureButton = GetHeroAdventureButton(page);
            var adventureAvailabe = await adventureButton.Locator("div.content").First.IsVisibleAsync();
            return adventureAvailabe;
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