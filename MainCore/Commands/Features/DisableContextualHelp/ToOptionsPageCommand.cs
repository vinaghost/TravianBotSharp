#pragma warning disable S1172

namespace MainCore.Commands.Features.DisableContextualHelp
{
    [Handler]
    public static partial class ToOptionsPageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken
            )
        {
            var (_, isFailed, element, errors) = await browser.GetElement(doc => OptionParser.GetOptionButton(doc), cancellationToken);
            if (isFailed) return Result.Fail(errors);

            var result = await browser.Click(element, cancellationToken);
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}