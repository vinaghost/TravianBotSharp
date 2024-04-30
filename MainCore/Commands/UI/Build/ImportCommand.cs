using MainCore.Commands.UI.Step;
using MainCore.DTO;
using System.Text.Json;

namespace MainCore.Commands.UI.Build
{
    public class ImportCommand : ByAccountVillageIdBase, IRequest
    {
        public ImportCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class ImportCommandHandler : IRequestHandler<ImportCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;
        private readonly ICommandHandler<ModifyJobCommand, List<JobDto>> _modifyJobCommand;

        public ImportCommandHandler(UnitOfRepository unitOfRepository, IDialogService dialogService, IMediator mediator, ICommandHandler<ModifyJobCommand, List<JobDto>> modifyJobCommand)
        {
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _mediator = mediator;
            _modifyJobCommand = modifyJobCommand;
        }

        public async Task Handle(ImportCommand request, CancellationToken cancellationToken)
        {
            var path = _dialogService.OpenFileDialog();
            List<JobDto> jobs;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path, cancellationToken);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString);
            }
            catch
            {
                _dialogService.ShowMessageBox("Warning", "Invalid file.");
                return;
            }

            var confirm = _dialogService.ShowConfirmBox("Warning", "TBS will remove resource field build job if its position doesn't match with current village.");
            if (!confirm) return;

            var accountId = request.AccountId;
            var villageId = request.VillageId;

            await _modifyJobCommand.Handle(new(villageId, jobs), cancellationToken);
            var modifiedJobs = _modifyJobCommand.Value;
            _unitOfRepository.JobRepository.AddRange(villageId, modifiedJobs);

            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Jobs imported");
        }
    }
}