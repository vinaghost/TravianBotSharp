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
        private readonly IAccountSettingRepository _accountSettingRepository;
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(IAccountSettingRepository accountSettingRepository, IDialogService dialogService)
        {
            _accountSettingRepository = accountSettingRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(ExportCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = _accountSettingRepository.Get(accountId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }
    }
}