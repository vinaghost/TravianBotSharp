using MainCore.Common.Models;
using MainCore.DTO;
using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class ExtractResourceFieldJobCommand : ByAccountVillageIdBase, ICommand
    {
        public JobDto Job { get; }

        public ExtractResourceFieldJobCommand(AccountId accountId, VillageId villageId, JobDto job) : base(accountId, villageId)
        {
            Job = job;
        }
    }

    public class ExtractResourceFieldJobCommandHandler : ICommandHandler<ExtractResourceFieldJobCommand>
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IMediator _mediator;

        public ExtractResourceFieldJobCommandHandler(IBuildingRepository buildingRepository, IJobRepository jobRepository, IMediator mediator)
        {
            _buildingRepository = buildingRepository;
            _jobRepository = jobRepository;
            _mediator = mediator;
        }

        public async Task<Result> Handle(ExtractResourceFieldJobCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var job = request.Job;

            var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);

            var normalBuildPlan = _buildingRepository.GetNormalBuildPlan(villageId, resourceBuildPlan);
            if (normalBuildPlan is null)
            {
                _jobRepository.Delete(job.Id);
            }
            else
            {
                _jobRepository.AddToTop(villageId, normalBuildPlan);
            }
            await _mediator.Publish(new JobUpdated(accountId, villageId), cancellationToken);
            return Result.Ok();
        }
    }
}