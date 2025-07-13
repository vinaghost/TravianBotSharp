namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteStartAdventureTaskCommand
    {
        public sealed record Command(StartAdventureTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser
            )
        {
            await Task.CompletedTask;
            var adventureDuration = AdventureParser.GetAdventureDuration(browser.Html);
            command.Task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);
        }
    }
}