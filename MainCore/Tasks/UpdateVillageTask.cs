using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Features;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateVillageTask : VillageTask
    {
        private readonly ITaskManager _taskManager;

        public UpdateVillageTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _mediator.Send(new UpdateDorf1Command(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var autoRefresh = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.AutoRefreshEnable);
            if (!autoRefresh) return;
            var seconds = _unitOfRepository.VillageSettingRepository.GetByName(VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Update village in {village}";
        }
    }
}