using FluentResults;
using MainCore.Commands.Base;
using MainCore.Commands.Validate;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MediatR;

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
        private readonly ICommandHandler<UpdateVillageListCommand> _updateVillageListCommand;
        private readonly ICommandHandler<UpdateDorfCommand> _updateDorfCommand;
        private readonly ICommandHandler<ValidateQuestCommand, bool> _validateQuestCommand;
        private readonly IMediator _mediator;

        public UpdateVillageInfoCommandHandler(ICommandHandler<UpdateDorfCommand> updateDorfCommand, ICommandHandler<UpdateAccountInfoCommand> updateAccountInfoCommand, ICommandHandler<ValidateQuestCommand, bool> validateQuestCommand, IMediator mediator, ICommandHandler<UpdateVillageListCommand> updateVillageListCommand)
        {
            _updateDorfCommand = updateDorfCommand;
            _updateAccountInfoCommand = updateAccountInfoCommand;
            _validateQuestCommand = validateQuestCommand;
            _mediator = mediator;
            _updateVillageListCommand = updateVillageListCommand;
        }

        public async Task<Result> Handle(UpdateVillageInfoCommand command, CancellationToken cancellationToken)
        {
            Result result;
            result = await _updateAccountInfoCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateVillageListCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _updateDorfCommand.Handle(new(command.AccountId, command.VillageId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _validateQuestCommand.Handle(new(command.AccountId), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            if (_validateQuestCommand.Value)
            {
                await _mediator.Publish(new QuestUpdated(command.AccountId, command.VillageId), cancellationToken);
            }
            return Result.Ok();
        }
    }
}