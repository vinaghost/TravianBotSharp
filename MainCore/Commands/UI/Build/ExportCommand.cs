using MainCore.Common.MediatR;
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
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;

        public ExportCommandHandler(UnitOfRepository unitOfRepository, IDialogService dialogService)
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
            jobs.ForEach(job => job.Id = JobId.Empty);
            var jsonString = JsonSerializer.Serialize(jobs);
            await File.WriteAllTextAsync(path, jsonString, cancellationToken);
            _dialogService.ShowMessageBox("Information", "Job list exported");
        }
    }
}