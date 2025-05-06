using MainCore.Constraints;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartActiveFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            DelayClickCommand.Handler delayClickCommand,
            GetActiveFarmsQuery.Handler getActiveFarmsQuery,
            CancellationToken cancellationToken)
        {
            

            var farmLists = await getActiveFarmsQuery.HandleAsync(new(command.AccountId), cancellationToken);
            if (farmLists.Count == 0) return Skip.NoActiveFarmlist;

            var html = browser.Html;

            foreach (var farmList in farmLists)
            {
                var startButton = FarmListParser.GetStartButton(html, farmList);
                if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                var result = await browser.Click(By.XPath(startButton.XPath));
                if (result.IsFailed) return result;

                await delayClickCommand.HandleAsync(new(command.AccountId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}