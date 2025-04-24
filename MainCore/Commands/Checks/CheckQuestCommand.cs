using MainCore.Commands.Abstract;

namespace MainCore.Commands.Checks
{
    [RegisterScoped<CheckQuestCommand>]
    public class CheckQuestCommand(IDataService dataService, QuestUpdated.Handler questUpdated) : CommandBase(dataService), ICommand
    {
        private readonly QuestUpdated.Handler _questUpdated = questUpdated;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var html = _dataService.ChromeBrowser.Html;
            if (!QuestParser.IsQuestClaimable(html)) return Result.Ok();

            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;

            await _questUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
            return Result.Ok();
        }
    }
}