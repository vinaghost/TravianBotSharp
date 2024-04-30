using MainCore.Commands.Base;
using MainCore.Common.MediatR;

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
        private readonly ICommandHandler<CloseBrowserCommand> _closeCommand;

        public SleepBrowserCommandHandler(ILogService logService, ICommandHandler<CloseBrowserCommand> closeCommand)
        {
            _logService = logService;
            _closeCommand = closeCommand;
        }

        public async Task<Result> Handle(SleepBrowserCommand command, CancellationToken cancellationToken)
        {
            Result result;
            result = await _closeCommand.Handle(new CloseBrowserCommand(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

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
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;
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