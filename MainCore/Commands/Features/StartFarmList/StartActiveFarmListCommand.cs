namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public static partial class StartActiveFarmListCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser,
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
                var startButton = FarmListParser.GetStartButton(browser.Html, farmList);
                if (startButton is null) return Retry.ButtonNotFound($"Start farm {farmList}");

                var result = await browser.Click(By.XPath(startButton.XPath));
                if (result.IsFailed) return result;

                await delayService.DelayClick(cancellationToken);
            }

            // Tüm farm list attack'lar baþlatýldýktan sonra 7 saniye bekle
            // Bu süre attack'larýn gönderilmesi ve sayfanýn stabilleþmesi için gerekli
            await Task.Delay(7000, cancellationToken);

            return Result.Ok();
        }
    }
}
