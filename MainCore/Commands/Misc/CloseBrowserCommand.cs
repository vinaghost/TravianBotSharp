using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Misc
{
    [Handler]
    public sealed partial class CloseBrowserCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(CloseBrowserCommand)}";
        }

        private async ValueTask HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            await browser.Shutdown();
        }
    }
}