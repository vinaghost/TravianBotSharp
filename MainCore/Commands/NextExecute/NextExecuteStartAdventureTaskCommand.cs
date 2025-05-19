namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors]
    public static partial class NextExecuteStartAdventureTaskCommand
    {
        private static async ValueTask HandleAsync(
            StartAdventureTask.Task task,
            IChromeBrowser browser,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var adventureDuration = AdventureParser.GetAdventureDuration(browser.Html);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);
            logger.Information("Schedule next run at {Time}", task.ExecuteAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}