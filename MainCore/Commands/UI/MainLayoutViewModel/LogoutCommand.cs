namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LogoutCommand
    {
        public sealed record Command(AccountId AccountId) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeManager chromeManager, ITaskManager taskManager,
            CancellationToken cancellationToken
            )
        {
            var accountId = command.AccountId;
            await taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await taskManager.StopCurrentTast(accountId);

            var chromeBrowser = chromeManager.Get(accountId);
            await chromeBrowser.Close();

            await taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}