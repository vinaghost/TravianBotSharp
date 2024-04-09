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
    public class UpdateHeroItemsCommand : ByAccountIdBase, IRequest
    {
        public UpdateHeroItemsCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateHeroItemsCommandHandler : UpdateCommandHandlerBase, IRequestHandler<UpdateHeroItemsCommand>
    {
        public UpdateHeroItemsCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task Handle(UpdateHeroItemsCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.HeroParser.GetItems(html);
            _unitOfRepository.HeroItemRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new HeroItemUpdated(command.AccountId), cancellationToken);
        }
    }
}