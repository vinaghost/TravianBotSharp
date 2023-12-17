using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;

namespace MainCore.Commands.Update
{
    public class UpdateVillageInfoCommand : ByAccountVillageIdBase, ICommand
    {
        public UpdateVillageInfoCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateVillageInfoCommandHandler : ICommandHandler<UpdateVillageInfoCommand>
    {
        private readonly ICommandHandler<UpdateAccountInfoCommand> _updateAccountInfoCommand;
        private readonly ICommandHandler<UpdateDorfCommand> _updateDorfCommand;

        public UpdateVillageInfoCommandHandler(ICommandHandler<UpdateDorfCommand> updateDorfCommand, ICommandHandler<UpdateAccountInfoCommand> updateAccountInfoCommand)
        {
            _updateDorfCommand = updateDorfCommand;
            _updateAccountInfoCommand = updateAccountInfoCommand;
        }

        public async Task<Result> Handle(UpdateVillageInfoCommand command, CancellationToken cancellationToken)
        {
            Result result;
            result = await _updateAccountInfoCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _updateDorfCommand.Handle(new(command.AccountId, command.VillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}