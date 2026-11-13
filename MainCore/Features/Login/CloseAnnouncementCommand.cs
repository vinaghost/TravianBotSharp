namespace MainCore.Features.Login;

[Handler]
public static partial class CloseAnnouncementCommand
{
    public sealed record Command(IChromeBrowser Browser) : ICommand
    {
        public string Name => nameof(CloseAnnouncementCommand).Humanize();
    }

    private static async ValueTask<Result> HandleAsync(Command command)
    {
        var browser = command.Browser;
        Result result;

        var continueButton = browser.CurrentPage.GetConstructionPlansContinueButton();
        if (await continueButton.CountAsync() == 0)
        {
            return Result.Ok();
        }

        result = await browser.Click(continueButton);
        if (result.IsFailed) return result;

        return Result.Ok();
    }
}