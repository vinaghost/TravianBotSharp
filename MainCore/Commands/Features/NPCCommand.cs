using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.NPC;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Features
{
    public class NPCCommand : ByAccountVillageIdBase, IRequest<Result>
    {
        public NPCCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class NPCCommandHandler : IRequestHandler<NPCCommand, Result>
    {
        private readonly ICommandHandler<OpenNPCDialogCommand> _openNPCDialogCommand;
        private readonly ICommandHandler<InputAmountCommand> _inputAmountCommand;
        private readonly ICommandHandler<RedeemCommand> _redeemCommand;

        public NPCCommandHandler(ICommandHandler<OpenNPCDialogCommand> openNPCDialogCommand, ICommandHandler<InputAmountCommand> inputAmountCommand, ICommandHandler<RedeemCommand> redeemCommand)
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
            result = await _openNPCDialogCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _inputAmountCommand.Handle(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _redeemCommand.Handle(new(accountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}