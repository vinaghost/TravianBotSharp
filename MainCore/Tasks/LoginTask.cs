using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Special;
using MainCore.Commands.Step.DisableContextualHelp;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public sealed class LoginTask : AccountTask
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfCommand _unitOfCommand;
        private readonly IValidateContextualHelpCommand _validateContextualHelpCommand;

        public LoginTask(IMediator mediator, IUnitOfCommand unitOfCommand, IValidateContextualHelpCommand validateContextualHelpCommand)
        {
            _mediator = mediator;
            _unitOfCommand = unitOfCommand;
            _validateContextualHelpCommand = validateContextualHelpCommand;
        }

        protected override  async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();
            Result result;
            result = await _mediator.Send(new LoginCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await DisableContextualHelp();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageListCommand.Execute(AccountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> DisableContextualHelp()
        {
            Result result;
            result = await _validateContextualHelpCommand.Execute(AccountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            if (!_validateContextualHelpCommand.Value) return Result.Ok();

            result = await _mediator.Send(new DisableContextualHelpCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = _unitOfCommand.ToDorfCommand.Execute(AccountId, 1);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}