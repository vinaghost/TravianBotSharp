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
    public class UpdateFarmListTask : AccountTask
    {
        public UpdateFarmListTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _mediator.Send(new ToFarmListPageCommand(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateFarmListCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update farm lists";
        }
    }
}