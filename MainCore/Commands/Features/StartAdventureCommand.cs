using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.StartAdventure;
using MainCore.Commands.Navigate;
using MainCore.Commands.Update;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Features
{
    public class StartAdventureCommand : ByAccountIdBase, IRequest<Result>
    {
        public StartAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class StartAdventureCommandHandler : IRequestHandler<StartAdventureCommand, Result>
    {
        private readonly ICommandHandler<ToAdventurePageCommand> _toAdventurePageCommand;
        private readonly ICommandHandler<ExploreAdventureCommand> _exploreAdventureCommand;
        private readonly ICommandHandler<UpdateAdventureCommand> _updateAdventureCommand;

        public StartAdventureCommandHandler(ICommandHandler<ToAdventurePageCommand> toAdventurePageCommand, ICommandHandler<ExploreAdventureCommand> exploreAdventureCommand, ICommandHandler<UpdateAdventureCommand> updateAdventureCommand)
        {
            _toAdventurePageCommand = toAdventurePageCommand;
            _exploreAdventureCommand = exploreAdventureCommand;
            _updateAdventureCommand = updateAdventureCommand;
        }

        public async Task<Result> Handle(StartAdventureCommand request, CancellationToken cancellationToken)
        {
            Result result;
            result = await _toAdventurePageCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _updateAdventureCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _exploreAdventureCommand.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}