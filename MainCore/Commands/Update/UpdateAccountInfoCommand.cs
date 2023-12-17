using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateAccountInfoCommand : ByAccountIdBase, ICommand
    {
        public UpdateAccountInfoCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateAccountInfoCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateAccountInfoCommand>
    {
        public UpdateAccountInfoCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateAccountInfoCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dto = _unitOfParser.AccountInfoParser.Get(html);
            _unitOfRepository.AccountInfoRepository.Update(command.AccountId, dto);
            await _mediator.Publish(new AccountInfoUpdated(command.AccountId), cancellationToken);

            if (_unitOfParser.HeroParser.CanStartAdventure(html))
            {
                await _mediator.Publish(new AdventureUpdated(command.AccountId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}