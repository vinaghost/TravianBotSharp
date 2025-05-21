using MainCore.Behaviors;

namespace MainCore.Commands.NextExecute
{
    [Handler]
    [Behaviors(typeof(NextExecuteLoggingBehaviors<,>))]
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
        }
    }
}