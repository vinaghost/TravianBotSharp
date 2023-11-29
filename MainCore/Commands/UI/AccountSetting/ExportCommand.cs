using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.UI.AccountSetting
{
    public class ExportCommand : IRequest
    {
        public AccountId AccountId { get; }

        public ExportCommand(AccountId accountId)
        {
            AccountId = accountId;
        }
    }

    public class ExportCommandHandler : IRequestHandler<ExportCommand>
    {
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(IUnitOfRepository unitOfRepository, IDialogService dialogService)
        {
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(ExportCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var path = _dialogService.SaveFileDialog();
            var settings = _unitOfRepository.AccountSettingRepository.Get(accountId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }
    }
}