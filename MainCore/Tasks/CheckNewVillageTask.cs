using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Update;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class CheckNewVillageTask : AccountTask
    {
        private readonly ICommandHandler<UpdateAccountInfoCommand> _updateAccountInfoCommand;
        private readonly ICommandHandler<UpdateVillageListCommand> _updateVillageListCommand;

        public CheckNewVillageTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<UpdateAccountInfoCommand> updateAccountInfoCommand, ICommandHandler<UpdateVillageListCommand> updateVillageListCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _updateAccountInfoCommand = updateAccountInfoCommand;
            _updateVillageListCommand = updateVillageListCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _updateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _updateVillageListCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Update new village after settle";
        }
    }
}