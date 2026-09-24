namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchVillageCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
           Command command,
           IChromeBrowser browser,
           CancellationToken cancellationToken
           )
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