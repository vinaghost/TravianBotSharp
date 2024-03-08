using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Navigate;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class UpdateAdventureTask : AccountTask
    {
        private readonly ICommandHandler<ToAdventurePageCommand> _toAdventurePageCommand;

        public UpdateAdventureTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ToAdventurePageCommand> toAdventurePageCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _toAdventurePageCommand = toAdventurePageCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _toAdventurePageCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateAdventureCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update adventure";
        }
    }
}