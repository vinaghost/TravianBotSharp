using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.UI.AccountSetting
{
    public class ExportCommand : ByAccountIdBase, IRequest
    {
        public ExportCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class ExportCommandHandler : IRequestHandler<ExportCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(UnitOfRepository unitOfRepository, IDialogService dialogService)
        {
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(ExportCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = _unitOfRepository.AccountSettingRepository.Get(accountId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }
    }
}