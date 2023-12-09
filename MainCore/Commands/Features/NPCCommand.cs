using FluentResults;
using MainCore.Commands.Features.Step.NPC;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Special
{
    public class NPCCommand : ByAccountVillageIdBase, IRequest<Result>
    {
        public NPCCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class NPCCommandHandler : IRequestHandler<NPCCommand, Result>
    {
        private readonly IOpenNPCDialogCommand _openNPCDialogCommand;
        private readonly IInputAmountCommand _inputAmountCommand;
        private readonly IRedeemCommand _redeemCommand;

        public NPCCommandHandler(IOpenNPCDialogCommand openNPCDialogCommand, IInputAmountCommand inputAmountCommand, IRedeemCommand redeemCommand)
        {
            _openNPCDialogCommand = openNPCDialogCommand;
            _inputAmountCommand = inputAmountCommand;
            _redeemCommand = redeemCommand;
        }

        public async Task<Result> Handle(NPCCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            Result result;
            result = await _openNPCDialogCommand.Execute(accountId, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _inputAmountCommand.Execute(accountId, villageId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _redeemCommand.Execute(accountId, cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}