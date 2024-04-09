using MainCore.Commands.Base;
using MainCore.Commands.Validate;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateVillageInfoCommand : ByAccountVillageIdBase, IRequest
    {
        public UpdateVillageInfoCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateVillageInfoCommandHandler : IRequestHandler<UpdateVillageInfoCommand>
    {
        private readonly ICommandHandler<UpdateAccountInfoCommand> _updateAccountInfoCommand;
        private readonly ICommandHandler<UpdateDorfCommand> _updateDorfCommand;
        private readonly ICommandHandler<ValidateQuestCommand, bool> _validateQuestCommand;

        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly IMediator _mediator;

        public UpdateVillageInfoCommandHandler(IMediator mediator)
        {
            _updateDorfCommand = updateDorfCommand;
            _updateAccountInfoCommand = updateAccountInfoCommand;
            _validateQuestCommand = validateQuestCommand;
            _mediator = mediator;
        }

        public async Task Handle(UpdateVillageInfoCommand command, CancellationToken cancellationToken)
        {
            await _updateAccountInfoCommand.Handle(new(command.AccountId), cancellationToken);
            await _updateDorfCommand.Handle(new(command.AccountId, command.VillageId), cancellationToken);
            await _validateQuestCommand.Handle(new(command.AccountId), cancellationToken);

            if (_validateQuestCommand.Value)
            {
                await _mediator.Publish(new QuestUpdated(command.AccountId, command.VillageId), cancellationToken);
            }
        }
    }
}