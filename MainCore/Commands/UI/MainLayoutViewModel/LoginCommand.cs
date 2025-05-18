using MainCore.Constraints;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeBrowser browser, ITaskManager taskManager, ITimerManager timerManager, ILogger logger,
            AccountInit.Handler accountInit, OpenBrowserCommand.Handler openBrowserCommand,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = command;

            logger.Information("Using connection {Proxy} to start chrome", access.Proxy);

            try
            {
                taskManager.SetStatus(accountId, StatusEnums.Starting);
                await openBrowserCommand.HandleAsync(new(accountId, access), cancellationToken);
            }
            catch
            {
                taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }

            await accountInit.HandleAsync(new(accountId), cancellationToken);

            timerManager.Start(accountId);
            taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}