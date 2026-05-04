namespace MainCore.Commands.Features
{
    [Handler]
    public static partial class SleepCommand
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        public static int CalculateSleepDurationMinutes(ISettingService settingService, AccountId accountId)
        {
            // pick random number between configured min/max
            var sleepTimeMinutes = settingService.ByName(accountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);

            // determine next day start, respecting configured work window
            var workStartHour = settingService.ByName(accountId, AccountSettingEnums.WorkStartHour);
            if (workStartHour < 0 || workStartHour > 23) workStartHour = 6;
            var workStartMinute = settingService.ByName(accountId, AccountSettingEnums.WorkStartMinute);
            if (workStartMinute < 0 || workStartMinute > 59) workStartMinute = 0;

            var now = DateTime.Now;
            var startToday = now.Date.AddHours(workStartHour).AddMinutes(workStartMinute);
            DateTime nextStart = now < startToday ? startToday : startToday.AddDays(1);

            var maxAllowed = (int)Math.Ceiling((nextStart - now).TotalMinutes);
            if (sleepTimeMinutes > maxAllowed) sleepTimeMinutes = maxAllowed;

            return sleepTimeMinutes;
        }

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            ISettingService settingService,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            await browser.Close();

            var sleepTimeMinutes = CalculateSleepDurationMinutes(settingService, command.AccountId);
            var sleepEnd = DateTime.Now.AddMinutes(sleepTimeMinutes);
            int lastMinute = 0;
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