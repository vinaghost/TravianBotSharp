using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
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
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(UnitOfRepository unitOfRepository, IDialogService dialogService)
        {
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
        }

        public async Task Handle(ExportCommand request, CancellationToken cancellationToken)
        {
            var villageId = request.VillageId;
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var settings = _unitOfRepository.VillageSettingRepository.Get(villageId);
            var jsonString = JsonSerializer.Serialize(settings);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Settings exported");
        }
    }
}