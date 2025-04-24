using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    [RegisterScoped<CheckAdventureCommand>]
    public class CheckAdventureCommand(IDataService dataService, AdventureUpdated.Handler adventureUpdated) : CommandBase(dataService), ICommand
    {
        private readonly AdventureUpdated.Handler _adventureUpdated = adventureUpdated;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var html = _dataService.ChromeBrowser.Html;
            if (!AdventureParser.CanStartAdventure(html)) return Result.Ok();

            var accountId = _dataService.AccountId;
            await _adventureUpdated.HandleAsync(new(accountId), cancellationToken);
            return Result.Ok();
        }
    }
}