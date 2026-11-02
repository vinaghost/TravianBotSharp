using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.StartAdventure
{
    [Handler]
    public sealed partial class CheckHeroReadyCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CheckHeroReadyCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var canStartAdventure = await AdventureParser.CanStartAdventure(browser.CurrentPage);
            return canStartAdventure;
        }
    }
}