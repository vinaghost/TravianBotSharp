using FluentResults;
using MainCore.Commands;
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
        private readonly IChromeManager _chromeManager;
        private readonly ITaskManager _taskManager;

        public UpdateVillageTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager, ITaskManager taskManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var url = chromeBrowser.CurrentUrl;
            Result result;
            await chromeBrowser.Refresh(CancellationToken);
            if (url.Contains("dorf1"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
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