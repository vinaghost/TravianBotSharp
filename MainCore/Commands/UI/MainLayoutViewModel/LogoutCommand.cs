using MainCore.Constraints;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LogoutCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser, ITaskManager taskManager,
            CancellationToken cancellationToken
            )
        {
            var accountId = command.AccountId;
            await taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await taskManager.StopCurrentTast(accountId);


            await browser.Close();

            await taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}