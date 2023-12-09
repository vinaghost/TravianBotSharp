using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;
using System.Text.Json;

namespace MainCore.Commands.Features.Step.UpgradeBuilding
{
    public class ExtractResourceFieldCommand : ByAccountVillageIdBase, ICommand
    {
        public JobDto Job { get; }

        public ExtractResourceFieldCommand(AccountId accountId, VillageId villageId, JobDto job) : base(accountId, villageId)
        {
            Job = job;
        }
    }

    [RegisterAsTransient]
    public class ExtractResourceFieldCommandHandler : ICommandHandler<ExtractResourceFieldCommand>
    {
        private readonly IJobRepository _jobRepository;
        private readonly IBuildingRepository _buildingRepository;
        private readonly IMediator _mediator;

        public ExtractResourceFieldCommandHandler(IJobRepository jobRepository, IBuildingRepository buildingRepository, IMediator mediator)
        {
            _jobRepository = jobRepository;
            _buildingRepository = buildingRepository;
            _mediator = mediator;
        }

        public async Task<Result> Handle(ExtractResourceFieldCommand command, CancellationToken cancellationToken)
        {
            var resourceBuildPlan = JsonSerializer.Deserialize<ResourceBuildPlan>(command.Job.Content);
            var normalBuildPlan = _buildingRepository.GetNormalBuildPlan(command.VillageId, resourceBuildPlan);
            if (normalBuildPlan is null)
            {
                _jobRepository.Delete(command.Job.Id);
            }
            else
            {
                _jobRepository.AddToTop(command.VillageId, normalBuildPlan);
            }
            await _mediator.Publish(new JobUpdated(command.AccountId, command.VillageId));
            return Result.Ok();
        }
    }
}