using MainCore.Commands.Base;

namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class SleepCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeManager chromeManager,
            IDbContextFactory<AppDbContext> contextFactory,
            ILogService logService,
            CancellationToken cancellationToken)
        {
            var chromeBrowser = chromeManager.Get(command.AccountId);
            await chromeBrowser.Close();

            using var context = await contextFactory.CreateDbContextAsync();
            var sleepTimeMinutes = context.ByName(command.AccountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
            var sleepEnd = DateTime.Now.AddMinutes(sleepTimeMinutes);
            int lastMinute = 0;
            var logger = logService.GetLogger(command.AccountId);
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();

                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);

                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    logger.Information("Chrome will reopen in {CurrentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }
    }
}