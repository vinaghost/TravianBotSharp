using System;
using System.Collections.Generic;
using System.Text;

namespace MainCore.Commands.Features.StartFarmList
{
    [Handler]
    public sealed partial class StartAllFarmlistCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(StartAllFarmlistCommand)}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);
            var farmLists = GetActiveFarms(command.AccountId);
            if (farmLists.Count == 0) return Skip.Error.WithError("No farmlist is active");
            Result result;
            foreach (var farmList in farmLists)
            {
                result = await browser.Click(FarmListParser.GetStartButton(browser.CurrentPage, farmList));
                if (result.IsFailed) return result;
            }
            return Result.Ok();
        }

        private List<FarmId> GetActiveFarms(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var farmLists = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.IsActive)
                .Select(x => new FarmId(x.Id))
                .ToList();
            return farmLists;
        }
    }
}