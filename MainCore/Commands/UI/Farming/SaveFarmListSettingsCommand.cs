using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.UI.Farming
{
    public class SaveFarmListSettingsCommand : ByAccountIdBase, IRequest
    {
        public Dictionary<AccountSettingEnums, int> Settings { get; }

        public SaveFarmListSettingsCommand(AccountId accountId, Dictionary<AccountSettingEnums, int> settings) : base(accountId)
        {
            settings = Settings;
        }
    }

    public class SaveCommandHandler : IRequestHandler<SaveFarmListSettingsCommand>
    {
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;

        public SaveCommandHandler(IUnitOfRepository unitOfRepository, IMediator mediator, IDialogService dialogService)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
            _dialogService = dialogService;
        }

        public async Task Handle(SaveFarmListSettingsCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var settings = request.Settings;

            _unitOfRepository.AccountSettingRepository.Update(accountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(accountId));

            _dialogService.ShowMessageBox("Information", "Settings saved");
        }
    }
}