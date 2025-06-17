using MainCore.Constraints;
using MainCore.Services;
using System;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public static partial class ExploreAdventureCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ILogger logger,
            ISettingService settingService,
            CancellationToken cancellationToken)
        {
            var html = browser.Html;

            if (!AdventureParser.CanStartAdventure(html)) return Skip.NoAdventure;

            var maxMinutes = settingService.ByName(command.AccountId, AccountSettingEnums.AdventureMaxTravelTime);
            var adventureButton = AdventureParser.GetAdventureButton(html, TimeSpan.FromMinutes(maxMinutes));
            if (adventureButton is null) return Skip.NoAdventure;
            logger.Information("Start adventure {Adventure}", AdventureParser.GetAdventureInfo(adventureButton));

            static bool ContinueShow(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                var continueButton = AdventureParser.GetContinueButton(doc);
                return continueButton is not null;
            }

            var result = await browser.Click(By.XPath(adventureButton.XPath));
            if (result.IsFailed) return result;

            result = await browser.Wait(ContinueShow, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}