using MainCore.Commands.Features.ClaimQuest;

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
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _collectRewardCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}