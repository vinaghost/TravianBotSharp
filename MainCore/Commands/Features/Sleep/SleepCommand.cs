using System;
using System.Collections.Generic;
using System.Text;

using MainCore.Infrasturecture.Extensions;

using Microsoft.EntityFrameworkCore.Internal;

namespace MainCore.Commands.Features.Sleep
{
    [Handler]
    public sealed partial class SleepCommand(
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand
        {
            public string Name => $"{nameof(SleepCommand)}";
        }

        private async ValueTask<Result> HandleAsync(Command command, CancellationToken cancellationToken)
        {
            var sleepEnd = GetSleepTime(command.AccountId);
            int lastMinute = 0;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    return Cancel.Error;
                }

                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    logger.Information("Chrome will reopen in {CurrentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }

        private DateTime GetSleepTime(AccountId accountId)
        {
            using var context = contextFactory.CreateDbContext();
            var sleepTime = context.ByName(
                accountId,
                AccountSettingEnums.SleepTimeMin,
                AccountSettingEnums.SleepTimeMax,
                60);
            var sleepEnd = DateTime.Now.AddSeconds(sleepTime);

            return sleepEnd;
        }
    }
}