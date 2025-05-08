using MainCore.Constraints;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LogoutCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser, ITaskManager taskManager,
            CancellationToken cancellationToken
            )
        {
            var accountId = command.AccountId;
            taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await taskManager.StopCurrentTask(accountId);


            await browser.Close();

            taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}