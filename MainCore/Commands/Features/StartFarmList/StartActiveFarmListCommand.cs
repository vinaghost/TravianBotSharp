namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartActiveFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            IDelayService delayService,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            var accountId = command.AccountId;
            var farmLists = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new FarmId(x.Id))
                .ToList();
            if (farmLists.Count == 0) return Skip.Error.WithError("No farmlist is active");

            foreach (var farmList in farmLists)
            {
                var result = await browser.Click(FarmListParser.GetStartButton(browser.CurrentPage, farmList));
                if (result.IsFailed) return result;

                await delayService.DelayClick(cancellationToken);
            }

            return Result.Ok();
        }
    }
}