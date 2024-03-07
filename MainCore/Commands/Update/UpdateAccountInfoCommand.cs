using FluentResults;
using HtmlAgilityPack;
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

            await UpdateAccount(command.AccountId, html, cancellationToken);
            await UpdateHero(command.AccountId, html, cancellationToken);

            return Result.Ok();
        }

        public async Task UpdateAccount(AccountId accountId, HtmlDocument doc, CancellationToken cancellationToken)
        {
            var dto = _unitOfParser.AccountInfoParser.Get(doc);
            _unitOfRepository.AccountInfoRepository.Update(accountId, dto);
            await _mediator.Publish(new AccountInfoUpdated(accountId), cancellationToken);
        }

        public async Task UpdateHero(AccountId accountId, HtmlDocument doc, CancellationToken cancellationToken)
        {
            var dto = _unitOfParser.HeroParser.Get(doc);
            _unitOfRepository.HeroRepository.Update(accountId, dto);
            await _mediator.Publish(new HeroUpdated(accountId), cancellationToken);

            if (_unitOfParser.HeroParser.CanStartAdventure(doc))
            {
                await _mediator.Publish(new AdventureUpdated(accountId), cancellationToken);
            }

            if (_unitOfParser.HeroParser.IsLevelUp(doc))
            {
                await _mediator.Publish(new HeroLevelUpdated(accountId), cancellationToken);
            }
            if (_unitOfParser.HeroParser.IsDead(doc))
            {
                await _mediator.Publish(new HeroDead(accountId), cancellationToken);
            }
        }
    }
}