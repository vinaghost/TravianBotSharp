using FluentResults;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.Features
{
    public class ToNPCPageCommand : ByAccountVillageIdBase, IRequest<Result>
    {
        public ToNPCPageCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class ToNPCPageCommandHandler : IRequestHandler<ToNPCPageCommand, Result>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly UnitOfCommand _unitOfCommand;

        public ToNPCPageCommandHandler(UnitOfRepository unitOfRepository, UnitOfCommand unitOfCommand)
        {
            _unitOfRepository = unitOfRepository;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(ToNPCPageCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            Result result;

            var market = _unitOfRepository.BuildingRepository.GetBuildingLocation(villageId, BuildingEnums.Marketplace);

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(accountId, market), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(accountId, 0), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}
