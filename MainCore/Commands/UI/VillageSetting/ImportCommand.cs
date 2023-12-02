using FluentValidation;
using MainCore.Common.Enums;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;
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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public ImportCommandHandler(IValidator<VillageSettingInput> villageSettingInputValidator, IUnitOfRepository unitOfRepository, IDialogService dialogService, IMediator mediator)
        {
            _villageSettingInputValidator = villageSettingInputValidator;
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _mediator = mediator;
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
            await Task.Run(() => _unitOfRepository.VillageSettingRepository.Update(villageId, settings), cancellationToken);
            await _mediator.Publish(new VillageSettingUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Settings imported");
        }
    }
}