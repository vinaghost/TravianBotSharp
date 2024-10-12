using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features
{
    [RegisterScoped<SleepCommand>]
    public class SleepCommand : CommandBase, ICommand
    {
        private readonly IGetSetting _getSetting;

        public SleepCommand(IDataService dataService, IGetSetting getSetting) : base(dataService)
        {
            _getSetting = getSetting;
        }

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            await chromeBrowser.Close();

            var logger = _dataService.Logger;
            var accountId = _dataService.AccountId;

            var sleepTimeMinutes = _getSetting.ByName(accountId, AccountSettingEnums.SleepTimeMin, AccountSettingEnums.SleepTimeMax);
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