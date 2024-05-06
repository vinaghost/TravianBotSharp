using System.Text.Json;

namespace MainCore.Commands.UI.VillageSetting
{
    public class ExportCommand : ByVillageIdBase, IRequest
    {
        public ExportCommand(VillageId villageId) : base(villageId)
        {
        }
    }

    public class ExportCommandHandler : IRequestHandler<ExportCommand>
    {
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task Handle(ExportCommand request, CancellationToken cancellationToken)
        {
            var villageId = request.VillageId;
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = new GetSetting().Get(villageId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }
    }
}