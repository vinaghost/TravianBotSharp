using MainCore.Commands.Base;
using MainCore.Common.MediatR;

namespace MainCore.Commands.Misc
{
    public class CheckQuestCommand : ByAccountVillageIdBase, ICommand
    {
        public CheckQuestCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class CheckQuestCommandHandler : ICommandHandler<CheckQuestCommand>
    {
        private readonly IMediator _mediator;
        private readonly IChromeManager _chromeManager;

        private readonly IQuestParser _questParser;

        public CheckQuestCommandHandler(IMediator mediator, IChromeManager chromeManager, IQuestParser questParser)
        {
            _mediator = mediator;
            _chromeManager = chromeManager;
            _questParser = questParser;
        }

        public async Task<Result> Handle(CheckQuestCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            if (_questParser.IsQuestClaimable(html))
            {
                await _mediator.Publish(new QuestUpdated(accountId, villageId), cancellationToken);
            }

            return Result.Ok();
        }
    }
}