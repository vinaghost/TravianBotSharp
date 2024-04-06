using FluentResults;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateBuildingTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;

        public UpdateBuildingTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var url = chromeBrowser.CurrentUrl;
            Result result;
            if (url.Contains("dorf1"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            }
            else
            {
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
                result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
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