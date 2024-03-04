using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Features;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class SetHeroPointTask : AccountTask
    {
        public SetHeroPointTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _unitOfCommand.ToHeroAttributeCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _mediator.Send(new SetHeroPointCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Set hero point task";
        }
    }
}