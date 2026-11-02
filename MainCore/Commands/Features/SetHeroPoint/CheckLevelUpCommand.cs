using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.SetHeroPoint
{
    [Handler]
    public sealed partial class CheckLevelUpCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CheckLevelUpCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var canSetHeroPoint = await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() > 0;
            return canSetHeroPoint;
        }
    }
}