using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    public class CheckAdventureCommand : AdventureCommand
    {
        private readonly IMediator _mediator;

        public CheckAdventureCommand(IMediator mediator = null)
        {
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            if (CanStartAdventure(html))
            {
                await _mediator.Publish(new AdventureUpdated(accountId), cancellationToken);
            }
        }
    }
}