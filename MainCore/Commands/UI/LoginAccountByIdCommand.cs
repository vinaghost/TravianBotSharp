using FluentResults;
using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI
{
    public class LoginAccountByIdCommand : ByAccountIdBase, IRequest<Result>
    {
        public LoginAccountByIdCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LoginAccountByIdCommandHandler : IRequestHandler<LoginAccountByIdCommand, Result>
    {
        private readonly ITaskManager _taskManager;
        private readonly ITimerManager _timerManager;

        private readonly IChooseAccessCommand _chooseAccessCommand;
        private readonly IOpenBrowserCommand _workCommand;
        private readonly ILogService _logService;
        private readonly IMediator _mediator;

        public LoginAccountByIdCommandHandler(ITaskManager taskManager, ITimerManager timerManager, IOpenBrowserCommand workCommand, IChooseAccessCommand chooseAccessCommand, ILogService logService, IMediator mediator)
        {
            _taskManager = taskManager;
            _timerManager = timerManager;
            _workCommand = workCommand;
            _chooseAccessCommand = chooseAccessCommand;
            _logService = logService;
            _mediator = mediator;
        }

        public async Task<Result> Handle(LoginAccountByIdCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            _taskManager.SetStatus(accountId, StatusEnums.Starting);

            Result result;
            result = await _chooseAccessCommand.Execute(accountId, true);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            var logger = _logService.GetLogger(accountId);
            var access = _chooseAccessCommand.Value;
            logger.Information("Using connection {proxy} to start chrome", access.Proxy);
            result = _workCommand.Execute(accountId, access);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            await _mediator.Publish(new AccountInit(accountId));

            _timerManager.Start(accountId);
            _taskManager.SetStatus(accountId, StatusEnums.Online);
            return Result.Ok();
        }
    }
}