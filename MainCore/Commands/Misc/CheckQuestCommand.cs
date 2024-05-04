using MainCore.Commands.Features.ClaimQuest;

namespace MainCore.Commands.Misc
{
    public class CheckQuestCommand : ByAccountVillageIdBase, ICommand
    {
        public IChromeBrowser ChromeBrowser { get; }

        public CheckQuestCommand(AccountId accountId, VillageId villageId, IChromeBrowser chromeBrowser) : base(accountId, villageId)
        {
            ChromeBrowser = chromeBrowser;
        }
    }

    public class CheckQuestCommandHandler : QuestCommand, ICommandHandler<CheckQuestCommand>
    {
        private readonly IMediator _mediator;

        public CheckQuestCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<Result> Handle(CheckQuestCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var chromeBrowser = request.ChromeBrowser; ;
            var html = chromeBrowser.Html;
            if (IsQuestClaimable(html))
            {
                await _mediator.Publish(new QuestUpdated(accountId, villageId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}