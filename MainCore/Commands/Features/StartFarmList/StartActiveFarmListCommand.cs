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
            if (farmLists.Count == 0) return Skip.NoActiveFarmlist;

            foreach (var farmList in farmLists)
            {
                var (_, isFailed, element, errors) = await browser.GetElement(doc => FarmListParser.GetStartButton(doc, farmList), cancellationToken);
                if (isFailed) return Result.Fail(errors);

                var result = await browser.Click(element, cancellationToken);
                if (result.IsFailed) return result;

                await delayService.DelayClick(cancellationToken);
            }

            return Result.Ok();
        }
    }
}