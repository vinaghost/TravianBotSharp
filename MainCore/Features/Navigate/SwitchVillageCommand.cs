namespace MainCore.Features.Navigate;

[Handler]
public static partial class SwitchVillageCommand
{
    public sealed record Command(IChromeBrowser Browser, VillageId VillageId) : ICommand
    {
        public string Name => $"{nameof(SwitchVillageCommand)} [{Description}]";
        private string Description => $"{VillageId}";
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var (browser, villageId) = command;

        var villageNode = browser.CurrentPage.GetVillageNode(villageId);
        Result result;

        result = await browser.Wait(villageNode);
        if (result.IsFailed) return result;

        if (await villageNode.IsActive()) return Result.Ok();

        result = await browser.Click(villageNode);
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}