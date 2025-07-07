namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartAdventureTaskCommand
    {
        private static async ValueTask HandleAsync(
            StartAdventureTask.Task task,
            IChromeBrowser browser
            )
        {
            await Task.CompletedTask;
            var adventureDuration = AdventureParser.GetAdventureDuration(browser.Html);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);
        }
    }
}