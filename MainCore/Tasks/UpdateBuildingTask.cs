using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features;
using MainCore.Commands.UI.Step;
using MainCore.Common.Errors;
using MainCore.DTO;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;
using System.Text.Json;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateBuildingTask : VillageTask
    {
        private readonly ICommandHandler<ModifyJobCommand, List<JobDto>> _modifyJobCommand;

        public UpdateBuildingTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ModifyJobCommand, List<JobDto>> modifyJobCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _modifyJobCommand = modifyJobCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            var isMissingBuilding = _unitOfRepository.VillageRepository.IsMissingBuilding(VillageId);
            result = await _mediator.Send(new UpdateBothDorfCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            if (!isMissingBuilding) return Result.Ok();

            var path = _unitOfRepository.AccountInfoRepository.GetTemplatePath(AccountId);
            if (string.IsNullOrEmpty(path)) return Result.Ok();

            List<JobDto> jobs;
            try
            {
                var jsonString = await File.ReadAllTextAsync(path);
                jobs = JsonSerializer.Deserialize<List<JobDto>>(jsonString);
            }
            catch
            {
                return Result.Ok();
            }

            await _modifyJobCommand.Handle(new(VillageId, jobs), CancellationToken);
            var modifiedJobs = _modifyJobCommand.Value;
            if (modifiedJobs.Count > 0)
            {
                _unitOfRepository.JobRepository.AddRange(VillageId, modifiedJobs);

                await _mediator.Publish(new JobUpdated(AccountId, VillageId), CancellationToken);
            }
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Update all buildings in {village}";
        }
    }
}