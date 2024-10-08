using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    [RegisterScoped<CheckQuestCommand>]
    public class CheckQuestCommand(DataService dataService, IMediator mediator) : CommandBase(dataService), ICommand
    {
        private readonly IMediator _mediator = mediator;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var html = _dataService.ChromeBrowser.Html;
            if (!QuestParser.IsQuestClaimable(html)) return Result.Ok();

            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;

            await _mediator.Publish(new QuestUpdated(accountId, villageId), cancellationToken);
            return Result.Ok();
        }
    }
}