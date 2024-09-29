using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class CheckAdventureCommand(DataService dataService, IMediator mediator) : CommandBase(dataService)
    {
        private readonly IMediator _mediator = mediator;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var html = _dataService.ChromeBrowser.Html;
            if (!AdventureParser.CanStartAdventure(html)) return Result.Ok();

            var accountId = _dataService.AccountId;
            await _mediator.Publish(new AdventureUpdated(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}