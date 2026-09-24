#pragma warning disable S1172

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class DisableContextualHelpCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken
            )
        {
            var result = await browser.Click(OptionParser.GetHideContextualHelpOption(browser.CurrentPage));
            if (result.IsFailed) return result;

            result = await browser.Click(OptionParser.GetSubmitButton(browser.CurrentPage));
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}