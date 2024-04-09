using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.General;
using MainCore.Commands.Navigate;
using MainCore.Commands.Update;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class ToFarmListPageCommand : ByAccountIdBase, ICommand
    {
        public ToFarmListPageCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class ToFarmListPageCommandHandler : ICommandHandler<ToFarmListPageCommand>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly IMediator _mediator;

        private readonly ICommandHandler<ToBuildingCommand> _toBuildingCommand;

        public ToFarmListPageCommandHandler(UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ToBuildingCommand> toBuildingCommand)
        {
            _unitOfRepository = unitOfRepository;
            _mediator = mediator;
            _toBuildingCommand = toBuildingCommand;
        }

        public async Task<Result> Handle(ToFarmListPageCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await _mediator.Send(new UpdateVillageListCommand(accountId), cancellationToken);

            var rallypointVillageId = _unitOfRepository.VillageRepository.GetVillageHasRallypoint(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            Result result;
            result = await _mediator.Send(new SwitchVillageCommand(accountId, rallypointVillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new ToDorfCommand(accountId, 2), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Send(new UpdateVillageInfoCommand(accountId, rallypointVillageId), cancellationToken);

            result = await _toBuildingCommand.Handle(new(accountId, 39), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new SwitchTabCommand(accountId, 4), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _mediator.Send(new DelayClickCommand(accountId), cancellationToken);

            await _mediator.Send(new UpdateFarmListCommand(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}