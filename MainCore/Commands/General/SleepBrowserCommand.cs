using FluentResults;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Services;

namespace MainCore.Commands.General
{
    [RegisterAsTransient]
    public class SleepBrowserCommand : ISleepBrowserCommand
    {
        private readonly ILogService _logService;
        private readonly ICloseBrowserCommand _closeCommand;

        public SleepBrowserCommand(ILogService logService, ICloseBrowserCommand closeCommand)
        {
            _logService = logService;
            _closeCommand = closeCommand;
        }

        public async Task<Result> Execute(AccountId accountId, TimeSpan sleepTime, CancellationToken cancellationToken)
        {
            Result result;
            result = _closeCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var sleepEnd = DateTime.Now.Add(sleepTime);
            result = await Sleep(accountId, sleepEnd, cancellationToken);
            return result;
        }

        private async Task<Result> Sleep(AccountId accountId, DateTime sleepEnd, CancellationToken cancellationToken)
        {
            var logger = _logService.GetLogger(accountId);
            int lastMinute = 0;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return new Cancel();
                var timeRemaining = sleepEnd - DateTime.Now;
                if (timeRemaining < TimeSpan.Zero) return Result.Ok();
                await Task.Delay(TimeSpan.FromSeconds(1));
                var currentMinute = (int)timeRemaining.TotalMinutes;
                if (lastMinute != currentMinute)
                {
                    logger.Information("Chrome will reopen in {currentMinute} mins", currentMinute);
                    lastMinute = currentMinute;
                }
            }
        }
    }
}