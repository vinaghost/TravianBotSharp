namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class SwitchVillageCommand(ChromeManager chromeManager)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IVillageCommand
        {
            public string Name => $"{nameof(SwitchVillageCommand)} [{Description}]";
            private string Description => $"{VillageId}";
        }

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var (accountId, villageId) = command;
            var browser = chromeManager.Get(accountId);

            var villageNode = VillagePanelParser.GetVillageNode(browser.CurrentPage, villageId);
            if (await villageNode.CountAsync() == 0) return Skip.Error.WithError("Village not found");

            if (await VillagePanelParser.IsActive(villageNode)) return Result.Ok();

            Result result;
            result = await browser.Click(villageNode);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}