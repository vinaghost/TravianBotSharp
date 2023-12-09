using FluentResults;
using MainCore.Commands.Features.Step.DisableContextualHelp;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MediatR;

namespace MainCore.Commands.Special
{
    public class DisableContextualHelpCommand : ByAccountIdBase, IRequest<Result>
    {
        public DisableContextualHelpCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class DisableContextualHelpCommandHandler : IRequestHandler<DisableContextualHelpCommand, Result>
    {
        private readonly IDisableOptionCommand _disableOptionCommand;
        private readonly IOptionsSubmitCommand _optionsSubmitCommand;
        private readonly IToOptionsPageCommand _toOptionsPageCommand;

        public DisableContextualHelpCommandHandler(IDisableOptionCommand disableOptionCommand, IOptionsSubmitCommand optionsSubmitCommand, IToOptionsPageCommand toOptionsPageCommand)
        {
            _disableOptionCommand = disableOptionCommand;
            _optionsSubmitCommand = optionsSubmitCommand;
            _toOptionsPageCommand = toOptionsPageCommand;
        }

        public async Task<Result> Handle(DisableContextualHelpCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            Result result;
            result = await _toOptionsPageCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _disableOptionCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _optionsSubmitCommand.Execute(accountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}