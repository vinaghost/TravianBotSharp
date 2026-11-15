using System.Globalization;

namespace MainCore.Features.StartAdventure;

public static class Parser
{
    public static async Task<TimeSpan> GetAdventureDuration(this IPage page)
    {
        var timer = page.Locator("#heroAdventure span.timerReact");
        var text = await timer.InnerTextAsync();
        if (string.IsNullOrEmpty(text)) return TimeSpan.Zero;
        var duration = TimeSpan.Parse(text, CultureInfo.InvariantCulture);
        return duration;
    }

    public static ILocator GetAdventureDurationSection(this IPage page)
    {
        var heroAdventure = page.Locator("#heroAdventure div.videoFeatureBonusBox.adventureDuration");
        return heroAdventure;
    }

    public static ILocator GetHeroAdventureButton(this IPage page)
    {
        var adventureButton = page.Locator("a.adventure.round.attention");
        return adventureButton;
    }

    public static ILocator GetContinueButton(this IPage page)
    {
        var continueButton = page.Locator("button.continue");
        return continueButton;
    }

    public record struct AdventureInfo(string Difficult, TimeSpan Duration, ILocator Button);

    public record struct RawAdventureDto(string Difficulty, string DurationText, int Index);

    public static async Task<List<AdventureInfo>> GetAdventureInfo(this IPage page)
    {
        var rowsLocator = page.Locator("#heroAdventure tbody tr");
        await rowsLocator.First.WaitForAsync();

        var rawAdventures = await page.EvaluateAsync<RawAdventureDto[]>(@"() => {
                const rows = document.querySelectorAll('#heroAdventure tbody tr');
                return Array.from(rows).map((row, index) => {
                    const diffEl = row.querySelector('td.difficulty i');
                    const durEl = row.querySelector('td.duration .duration');

                    const diffClass = diffEl ? (diffEl.getAttribute('class') || '') : '';
                    const difficulty = diffClass ? diffClass.replace('difficulty_', '') : 'unknown';

                    const durationText = durEl ? (durEl.innerText || durEl.textContent).trim() : '00:00:00';

                    return {
                        Difficulty: difficulty,
                        DurationText: durationText,
                        Index: index
                    };
                });
            }") ?? throw new InvalidOperationException("Failed to retrieve or parse adventure data from the page.");

        var adventureInfoList = new List<AdventureInfo>(rawAdventures.Length);

        foreach (var raw in rawAdventures)
        {
            var duration = TimeSpan.Parse(raw.DurationText, CultureInfo.InvariantCulture);
            var buttonLocator = rowsLocator.Nth(raw.Index).Locator("td.button button");

            adventureInfoList.Add(new AdventureInfo(raw.Difficulty, duration, buttonLocator));
        }

        return adventureInfoList;
    }

    public record struct HeroState(int Health, bool IsAtHome, bool IsAdventureReady);

    public static async Task<HeroState> GetHeroState(this IPage page)
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
}