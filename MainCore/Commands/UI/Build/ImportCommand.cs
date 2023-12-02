using MainCore.Common.MediatR;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;
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
        private readonly IUnitOfRepository _unitOfRepository;
        private readonly IDialogService _dialogService;
        private readonly IMediator _mediator;

        public ImportCommandHandler(IUnitOfRepository unitOfRepository, IDialogService dialogService, IMediator mediator)
        {
            _unitOfRepository = unitOfRepository;
            _dialogService = dialogService;
            _mediator = mediator;
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
            var accountId = request.AccountId;
            var villageId = request.VillageId;

            _unitOfRepository.JobRepository.AddRange(villageId, jobs);

            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);

            _dialogService.ShowMessageBox("Information", "Jobs imported");
        }
    }
}