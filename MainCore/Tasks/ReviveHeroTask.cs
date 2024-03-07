using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features;
using MainCore.Commands.Validate;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class ReviveHeroTask : AccountTask
    {
        private readonly ICommandHandler<ValidateDeathCommand, bool> _validateDeathCommand;

        public ReviveHeroTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ValidateDeathCommand, bool> validateDeathCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _validateDeathCommand = validateDeathCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1, true), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _validateDeathCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var isDead = _validateDeathCommand.Value;
            if (!isDead) return Result.Ok();

            result = await _unitOfCommand.ToHeroAttributeCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _mediator.Send(new ReviveHeroCommand(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Revive hero task";
        }
    }
}