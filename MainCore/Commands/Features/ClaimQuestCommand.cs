using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.ClaimQuest;
using MainCore.Commands.Navigate;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Features
{
    public class ClaimQuestCommand : ByAccountIdBase, IRequest<Result>
    {
        public ClaimQuestCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class ClaimQuestCommandHandler : IRequestHandler<ClaimQuestCommand, Result>
    {
        private readonly ICommandHandler<ToQuestPageCommand> _toQuestPageCommand;
        private readonly ICommandHandler<CollectRewardCommand> _collectRewardCommand;

        public ClaimQuestCommandHandler(ICommandHandler<ToQuestPageCommand> toQuestPageCommand, ICommandHandler<CollectRewardCommand> collectRewardCommand)
        {
            _toQuestPageCommand = toQuestPageCommand;
            _collectRewardCommand = collectRewardCommand;
        }

        public async Task<Result> Handle(ClaimQuestCommand request, CancellationToken cancellationToken)
        {
            Result result;
            result = await _toQuestPageCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _collectRewardCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}