using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Update;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateExpansionSlotTask : VillageTask
    {
        private readonly ICommandHandler<UpdateExpansionSlotCommand> _updateExpansionSlotCommand;

        public UpdateExpansionSlotTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<UpdateExpansionSlotCommand> updateExpansionSlotCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _updateExpansionSlotCommand = updateExpansionSlotCommand;
        }

        protected override async Task<Result> Execute()
        {
            var location = _unitOfRepository.BuildingRepository.GetSettleLocation(VillageId);
            if (location == default)
            {
                return Result.Fail(new Skip("There is no building for settle"));
            }
            Result result;

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, location), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 4), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _updateExpansionSlotCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Update expansion slot task in {village}";
        }
    }
}