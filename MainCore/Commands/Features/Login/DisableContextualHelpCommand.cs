using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.Login
{
    [Handler]
    public sealed partial class DisableContextualHelpCommand(
        ToDorfCommand.Handler toDorfCommand,
        ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(DisableContextualHelpCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            Result result;

            var contextualHelpEnable = await OptionParser.IsContextualHelpEnable(browser.CurrentPage);
            if (!contextualHelpEnable) return Result.Ok();

            result = await browser.Click(OptionParser.GetOptionButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetHideContextualHelpOption(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetSubmitButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await toDorfCommand.HandleAsync(new(command.AccountId, 0));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}