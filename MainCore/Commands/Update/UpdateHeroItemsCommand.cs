using FluentResults;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    [RegisterAsTransient]
    public class UpdateHeroItemsCommand : UpdateCommandBase, IUpdateHeroItemsCommand
    {
        public UpdateHeroItemsCommand(IChromeManager chromeManager, IMediator mediator, IUnitOfRepository unitOfRepository, IUnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Execute(AccountId accountId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.HeroParser.Get(html);
            _unitOfRepository.HeroItemRepository.Update(accountId, dtos.ToList());
            await _mediator.Publish(new HeroItemUpdated(accountId));
            return Result.Ok();
        }
    }
}