using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Farming
{
    public class SaveFarmListSettingsCommand : ByAccountIdBase, IRequest
    {
        public SaveFarmListSettingsCommand(AccountId accountId, FarmListSettingInput farmListSettingInput) : base(accountId)
        {
            FarmListSettingInput = farmListSettingInput;
        }

        public FarmListSettingInput FarmListSettingInput { get; }
    }

    public class SaveCommandHandler : IRequestHandler<SaveFarmListSettingsCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;
        private readonly IDialogService _dialogService;

        private readonly IValidator<FarmListSettingInput> _farmListSettingInputValidator;

        public SaveCommandHandler(UnitOfRepository unitOfRepository, IMediator mediator, IDialogService dialogService, IValidator<FarmListSettingInput> farmListSettingInputValidator)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
            _dialogService = dialogService;
            _farmListSettingInputValidator = farmListSettingInputValidator;
        }

        public async Task Handle(SaveFarmListSettingsCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var farmListSettingInput = request.FarmListSettingInput;
            var result = _farmListSettingInputValidator.Validate(farmListSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            var settings = farmListSettingInput.Get();
            _accountSettingRepository.Update(accountId, settings);
            await _mediator.Publish(new AccountSettingUpdated(accountId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings saved");
        }
    }
}