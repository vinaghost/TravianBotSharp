namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            IChromeManager chromeManager, ITaskManager taskManager, ITimerManager timerManager, ILogService logService,
            AccountInit.Handler accountInit, OpenBrowserCommand.Handler openBrowserCommand,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = command;
            var logger = logService.GetLogger(accountId);
            logger.Information("Using connection {Proxy} to start chrome", access.Proxy);

            try
            {
                await taskManager.SetStatus(accountId, StatusEnums.Starting);
                await openBrowserCommand.HandleAsync(new(accountId, access), cancellationToken);
            }
            catch
            {
                await taskManager.SetStatus(accountId, StatusEnums.Offline);
                return;
            }

            await accountInit.HandleAsync(new(accountId), cancellationToken);

            timerManager.Start(accountId);
            await taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}