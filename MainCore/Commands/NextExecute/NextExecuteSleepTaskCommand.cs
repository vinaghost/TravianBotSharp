namespace MainCore.Commands.NextExecute
{
    [Handler]
    public static partial class NextExecuteSleepTaskCommand
    {
        public sealed record Command(SleepTask.Task Task) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            ISettingService settingService
            )
        {
            await Task.CompletedTask;
            
            // retrieve static work hours (defaults: 6am to 10pm if not set)
            var workStartHour = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.WorkStartHour);
            if (workStartHour < 0 || workStartHour > 23) workStartHour = 6;

            var workStartMinute = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.WorkStartMinute);
            if (workStartMinute < 0 || workStartMinute > 59) workStartMinute = 0;

            var workEndHour = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.WorkEndHour);
            if (workEndHour < 0 || workEndHour > 23) workEndHour = 22;

            var workEndMinute = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.WorkEndMinute);
            if (workEndMinute < 0 || workEndMinute > 59) workEndMinute = 0;

            // randomness range from 0 (no offset) up to configured max
            var random = new Random();
            var maxOffset = settingService.ByName(
                command.Task.AccountId,
                AccountSettingEnums.SleepRandomMinute);
            if (maxOffset < 0) maxOffset = 0;
            var randomMinute = maxOffset == 0 ? 0 : random.Next(0, maxOffset);

            var now = DateTime.Now;
            DateTime nextSleepTime;

            var startToday = now.Date.AddHours(workStartHour).AddMinutes(workStartMinute);
            var endToday = now.Date.AddHours(workEndHour).AddMinutes(workEndMinute);

            if (now < startToday)
            {
                // before work start, sleep until work start today
                nextSleepTime = startToday.AddMinutes(randomMinute);
            }
            else if (now >= endToday)
            {
                // after work end, sleep until work start tomorrow
                nextSleepTime = startToday.AddDays(1).AddMinutes(randomMinute);
            }
            else
            {
                // during work hours, sleep until end of work today
                nextSleepTime = endToday.AddMinutes(randomMinute);
            }

            command.Task.ExecuteAt = nextSleepTime;
        }
    }
}