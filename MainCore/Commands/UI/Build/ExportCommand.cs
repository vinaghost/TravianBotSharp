using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.UI.Build
{
    public class ExportCommand : ByVillageIdBase, IRequest
    {
        public ExportCommand(VillageId villageId) : base(villageId)
        {
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
            var path = _dialogService.SaveFileDialog();
            if (string.IsNullOrEmpty(path)) return;
            var villageId = request.VillageId;
            var jobs = _unitOfRepository.JobRepository.GetJobs(villageId);

            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Job list exported");
        }
    }
}