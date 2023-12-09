using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.DisableContextualHelp;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Features
{
    public class DisableContextualHelpCommand : ByAccountIdBase, IRequest<Result>
    {
        public DisableContextualHelpCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class DisableContextualHelpCommandHandler : IRequestHandler<DisableContextualHelpCommand, Result>
    {
        private readonly ICommandHandler<DisableOptionCommand> _disableOptionCommandHandler;
        private readonly ICommandHandler<OptionsSubmitCommand> _optionsSubmitCommandHandler;
        private readonly ICommandHandler<ToOptionsPageCommand> _toOptionsPageCommandHandler;

        public DisableContextualHelpCommandHandler(ICommandHandler<DisableOptionCommand> disableOptionCommandHandler, ICommandHandler<OptionsSubmitCommand> optionsSubmitCommandHandler, ICommandHandler<ToOptionsPageCommand> toOptionsPageCommandHandler)
        {
            _disableOptionCommandHandler = disableOptionCommandHandler;
            _optionsSubmitCommandHandler = optionsSubmitCommandHandler;
            _toOptionsPageCommandHandler = toOptionsPageCommandHandler;
        }

        public async Task<Result> Handle(DisableContextualHelpCommand request, CancellationToken cancellationToken)
        {
            Result result;
            result = await _toOptionsPageCommandHandler.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _disableOptionCommandHandler.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _optionsSubmitCommandHandler.Handle(new(request.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}