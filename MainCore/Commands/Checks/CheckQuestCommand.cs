using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class CheckQuestCommand(DataService dataService, IMediator mediator) : CommandBase(dataService)
    {
        private readonly IMediator _mediator = mediator;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
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