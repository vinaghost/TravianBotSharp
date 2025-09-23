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
            var (_, isFailed, element, errors) = await browser.GetElement(doc => OptionParser.GetHideContextualHelpOption(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            (_, isFailed, element, errors) = await browser.GetElement(doc => OptionParser.GetSubmitButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}