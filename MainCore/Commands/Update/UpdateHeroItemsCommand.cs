using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Update
{
    public class UpdateHeroItemsCommand : ByAccountIdBase, ICommand
    {
        public UpdateHeroItemsCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateHeroItemsCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateHeroItemsCommand>
    {
        public UpdateHeroItemsCommandHandler(IChromeManager chromeManager, IMediator mediator, UnitOfRepository unitOfRepository, UnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateHeroItemsCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var dtos = _unitOfParser.HeroParser.GetItems(html);
            _unitOfRepository.HeroItemRepository.Update(command.AccountId, dtos.ToList());
            await _mediator.Publish(new HeroItemUpdated(command.AccountId), cancellationToken);
            return Result.Ok();
        }
    }
}