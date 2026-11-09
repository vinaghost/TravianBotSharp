namespace MainCore.Features.ClaimVillageQuest;

[Handler]
public static partial class OpenVillageQuestPageCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(OpenVillageQuestPageCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;

        var questMaster = browser.CurrentPage.GetQuestMaster();
        var result = await browser.Click(questMaster);
        if (result.IsFailed) return result;

        result = await browser.WaitPageChanged("tasks");
        if (result.IsFailed) return result;

        var questItems = browser.CurrentPage.GetVillageQuestItems();
        result = await browser.Wait(questItems.First);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}