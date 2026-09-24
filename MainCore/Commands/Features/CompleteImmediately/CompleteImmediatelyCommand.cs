#pragma warning disable S1172

namespace MainCore.Commands.Features.CompleteImmediately
{
    [Handler]
    public static partial class CompleteImmediatelyCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            CancellationToken cancellationToken)
        {
            var oldQueueCount = await BuildingLayoutParser.CountQueueBuilding(browser.CurrentPage);
            if (oldQueueCount == 0) return Result.Ok();

            var completeButton = CompleteImmediatelyParser.GetCompleteButton(browser.CurrentPage);
            var result = await browser.Click(completeButton);
            if (result.IsFailed) return result;

            var confirmButton = CompleteImmediatelyParser.GetConfirmButton(browser.CurrentPage);
            result = await browser.Click(confirmButton);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}