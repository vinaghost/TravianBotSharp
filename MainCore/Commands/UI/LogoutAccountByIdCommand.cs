using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI
{
    public class LogoutAccountByIdCommand : ByAccountIdBase, IRequest
    {
        public LogoutAccountByIdCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LogoutAccountByIdCommandHandler : IRequestHandler<LogoutAccountByIdCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ICloseBrowserCommand _closeCommand;

        public LogoutAccountByIdCommandHandler(ITaskManager taskManager, ICloseBrowserCommand closeCommand)
        {
            _taskManager = taskManager;
            _closeCommand = closeCommand;
        }

        public async Task Handle(LogoutAccountByIdCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;

            _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            await Task.Run(() => _closeCommand.Execute(accountId));

            _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}