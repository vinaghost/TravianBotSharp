using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Services;

namespace MainCore.Commands.General
{
    public class SleepBrowserCommand : ByAccountIdBase, ICommand
    {
        public TimeSpan SleepTime { get; }

        public SleepBrowserCommand(AccountId accountId, TimeSpan sleepTime) : base(accountId)
        {
            SleepTime = sleepTime;
        }
    }

    [RegisterAsTransient]
    public class SleepBrowserCommandHandler : ICommandHandler<SleepBrowserCommand>
    {
        private readonly ILogService _logService;
        private readonly CloseBrowserCommandHandler _closeCommandHandler;

        public SleepBrowserCommandHandler(ILogService logService, CloseBrowserCommandHandler closeCommandHandler)
        {
            _logService = logService;
            _closeCommandHandler = closeCommandHandler;
        }

        public async Task<Result> Handle(SleepBrowserCommand command, CancellationToken cancellationToken)
        {
            Result result;
            result = await _closeCommandHandler.Handle(new CloseBrowserCommand(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var sleepEnd = DateTime.Now.Add(command.SleepTime);
            result = await Sleep(command.AccountId, sleepEnd, cancellationToken);
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
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
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