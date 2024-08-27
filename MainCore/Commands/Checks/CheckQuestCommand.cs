using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    public class CheckQuestCommand : QuestCommand
    {
        private readonly IMediator _mediator;

        public CheckQuestCommand(IMediator mediator = null)
        {
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            if (IsQuestClaimable(html))
            {
                await _mediator.Publish(new QuestUpdated(accountId, villageId), cancellationToken);
            }
        }
    }
}