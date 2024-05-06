using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.VillageSetting
{
    public class SaveCommand : ByAccountVillageIdBase, IRequest
    {
        public SaveCommand(AccountId accountId, VillageId villageId, VillageSettingInput villageSettingInput) : base(accountId, villageId)
        {
            VillageSettingInput = villageSettingInput;
        }

        public VillageSettingInput VillageSettingInput { get; }
    }

    public class SaveCommandHandler : IRequestHandler<SaveCommand>
    {
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public SaveCommandHandler(IValidator<VillageSettingInput> villageSettingInputValidator, IDialogService dialogService, IMediator mediator, IVillageSettingRepository villageSettingRepository)
        {
            _villageSettingInputValidator = villageSettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _villageSettingRepository = villageSettingRepository;
        }

        public async Task Handle(SaveCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageSettingInput = request.VillageSettingInput;
            var result = _villageSettingInputValidator.Validate(villageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var villageId = request.VillageId;
            var settings = villageSettingInput.Get();
            new SetVillageSettingCommand().Execute(villageId, settings);
            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings saved");
        }
    }
}