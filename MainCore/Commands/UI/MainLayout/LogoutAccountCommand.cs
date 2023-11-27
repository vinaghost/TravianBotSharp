using MainCore.Commands.General;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI.MainLayout
{
    public class LogoutAccountCommand : ByAccountIdBase, IRequest
    {
        public LogoutAccountCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class LogoutAccountCommandHandler : IRequestHandler<LogoutAccountCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly ICloseBrowserCommand _closeCommand;

        public LogoutAccountCommandHandler(ITaskManager taskManager, ICloseBrowserCommand closeCommand)
        {
            _taskManager = taskManager;
            _closeCommand = closeCommand;
        }

        public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;

            _taskManager.SetStatus(accountId, StatusEnums.Stopping);
            await _taskManager.StopCurrentTask(accountId);

            await Task.Run(() => _closeCommand.Execute(accountId));

            _taskManager.SetStatus(accountId, StatusEnums.Offline);
        }
    }
}