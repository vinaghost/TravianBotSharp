namespace MainCore.Commands.Navigate
{
    [Handler]
    public sealed partial class SwitchVillageCommand(IChromeBrowser browser)
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private async ValueTask<Result> HandleAsync(Command command)
        {
            var villageId = command.VillageId;

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