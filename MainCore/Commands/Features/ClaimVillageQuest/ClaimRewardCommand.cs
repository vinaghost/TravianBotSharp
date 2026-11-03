using MainCore.Commands.Navigate;

namespace MainCore.Commands.Features.ClaimVillageQuest
{
    [Handler]
    public sealed partial class ClaimRewardCommand(
        ChromeManager chromeManager,
        SwitchTabCommand.Handler switchTabCommand)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => nameof(ClaimRewardCommand).Humanize();
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var browser = chromeManager.Get(command.AccountId);

            Result result;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Cancel.Error;
                }

                var quest = QuestParser.GetQuestCollectButton(browser.CurrentPage);

                if (await quest.CountAsync() == 0)
                {
                    await Task.Delay(1000, CancellationToken.None);
                    if (await quest.CountAsync() == 0)
                    {
                        result = await switchTabCommand.HandleAsync(new(command.AccountId, 1), cancellationToken);
                        if (result.IsFailed) return result;
                    }

                    if (await quest.CountAsync() == 0)
                    {
                        return Result.Ok();
                    }
                }

                result = await browser.Click(quest.First);
                if (result.IsFailed) return result;
            }
            while (await QuestParser.IsVillageQuestClaimable(browser.CurrentPage));

            return Result.Ok();
        }
    }
}