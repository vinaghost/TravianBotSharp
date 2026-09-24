#pragma warning disable S1172

namespace MainCore.Commands.Features.ClaimQuest
{
    [Handler]
    public static partial class ToQuestPageCommand
    {
        public sealed record Command : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser)
        {
            var questMaster = QuestParser.GetQuestMaster(browser.CurrentPage);

            var result = await browser.Click(questMaster);
            if (result.IsFailed) return result;

            result = await browser.WaitPageChanged("tasks");
            if (result.IsFailed) return result;

            return Result.Ok();
        }
    }
}