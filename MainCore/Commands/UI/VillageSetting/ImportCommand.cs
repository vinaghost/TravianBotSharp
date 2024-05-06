using FluentValidation;
using MainCore.UI.Models.Input;
using System.Text.Json;

namespace MainCore.Commands.UI.VillageSetting
{
    public class ImportCommand : ByAccountVillageIdBase, IRequest
    {
        public ImportCommand(AccountId accountId, VillageId villageId, VillageSettingInput villageSettingInput) : base(accountId, villageId)
        {
            VillageSettingInput = villageSettingInput;
        }

        public VillageSettingInput VillageSettingInput { get; }
    }

    public class ImportCommandHandler : IRequestHandler<ImportCommand>
    {
        private readonly IValidator<VillageSettingInput> _villageSettingInputValidator;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public ImportCommandHandler(IValidator<VillageSettingInput> villageSettingInputValidator, IDialogService dialogService, IMediator mediator, IVillageSettingRepository villageSettingRepository)
        {
            _villageSettingInputValidator = villageSettingInputValidator;
            _dialogService = dialogService;
            _mediator = mediator;
            _villageSettingRepository = villageSettingRepository;
        }

        public async Task Handle(ImportCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageSettingInput = request.VillageSettingInput;

            var path = _dialogService.OpenFileDialog();
            Dictionary<VillageSettingEnums, int> settings;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path, cancellationToken);
                settings = JsonSerializer.Deserialize<Dictionary<VillageSettingEnums, int>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            villageSettingInput.Set(settings);
            var result = _villageSettingInputValidator.Validate(villageSettingInput);
            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }
            var villageId = request.VillageId;
            settings = villageSettingInput.Get();
            new SetVillageSettingCommand().Execute(villageId, settings);
            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }
    }
}