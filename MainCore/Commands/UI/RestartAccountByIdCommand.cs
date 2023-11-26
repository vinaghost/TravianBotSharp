using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI
{
    public class RestartAccountByIdCommand : ByAccountIdBase, IRequest
    {
        public RestartAccountByIdCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class RestartCommandHandler : IRequestHandler<RestartAccountByIdCommand>
    {
        private readonly ITaskManager _taskManager;
        private readonly IDialogService _dialogService;
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        public RestartCommandHandler(ITaskManager taskManager, IDialogService dialogService, IUnitOfRepository unitOfRepository, IMediator mediator)
        {
            _taskManager = taskManager;
            _dialogService = dialogService;
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
        }

        public async Task Handle(RestartAccountByIdCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var status = _taskManager.GetStatus(accountId);

            switch (status)
            {
                case StatusEnums.Offline:
                case StatusEnums.Starting:
                case StatusEnums.Pausing:
                case StatusEnums.Stopping:
                    _dialogService.ShowMessageBox("Information", $"Account is {status}");
                    return;

                case StatusEnums.Online:
                    _dialogService.ShowMessageBox("Information", $"Account should be paused first");
                    return;

                case StatusEnums.Paused:
                    await Handle(accountId);
                    return;
            }
        }

        private async Task Handle(AccountId accountId)
        {
            _taskManager.SetStatus(accountId, StatusEnums.Starting);
            await _taskManager.Clear(accountId);

            await _mediator.Publish(new AccountInit(accountId));

            _taskManager.SetStatus(accountId, StatusEnums.Online);
        }
    }
}