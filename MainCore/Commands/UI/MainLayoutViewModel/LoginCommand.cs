namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class LoginCommand
    {
        public sealed record Command(AccountId AccountId, AccessDto Access) : IAccountCommand;

        private static async ValueTask HandleAsync(
            Command command,
            ITaskManager taskManager,
            ITimerManager timerManager,
            ILogger logger,
            IRxQueue rxQueue,
            OpenBrowserCommand.Handler openBrowserCommand,
            CancellationToken cancellationToken
            )
        {
            var (accountId, access) = command;

            logger.Information("Using connection {Proxy} to start browser", access.Proxy);

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

            timerManager.Start(accountId);
            taskManager.SetStatus(accountId, StatusEnums.Online);
            rxQueue.Enqueue(new AccountInit(accountId));
        }
    }
}
