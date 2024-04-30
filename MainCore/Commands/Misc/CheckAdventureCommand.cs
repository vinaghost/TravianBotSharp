using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Misc
{
    public class CheckAdventureCommand : ByAccountIdBase, ICommand
    {
        public CheckAdventureCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class CheckAdventureCommandHandler : ICommandHandler<CheckAdventureCommand>
    {
        private readonly IMediator _mediator;
        private readonly IChromeManager _chromeManager;

        private readonly IHeroParser _heroParser;

        public CheckAdventureCommandHandler(IMediator mediator, IChromeManager chromeManager, IHeroParser heroParser)
        {
            _mediator = mediator;
            _chromeManager = chromeManager;
            _heroParser = heroParser;
        }

        public async Task<Result> Handle(CheckAdventureCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            if (_heroParser.CanStartAdventure(html))
            {
                await _mediator.Publish(new AdventureUpdated(accountId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}