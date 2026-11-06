using MainCore.Commands.Navigate;
using MainCore.Commands.Update;

namespace MainCore.Behaviors
{
    public class VillageTaskBehavior<TRequest, TResponse>(
        ChromeManager chromeManager,
        SwitchVillageCommand.Handler switchVillageCommand,
        UpdateStorageCommand.Handler updateStorageCommand,
        ToDorfCommand.Handler toDorfCommand,
        UpdateBuildingCommand.Handler updateBuildingCommand,
        UpdateQuestCommand.Handler updateQuestCommand)
            : Behavior<TRequest, TResponse>
                where TRequest : VillageTask
                where TResponse : Result
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var (accountId, villageId) = request;
            var browser = chromeManager.Get(accountId);
            Result result;
            result = await switchVillageCommand.HandleAsync(new(browser, villageId), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            var response = await Next(request, cancellationToken);

            if (response.IsFailed && !response.HasError<Skip>()) return response;

            result = await toDorfCommand.HandleAsync(new(browser, 2), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            result = await updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            await updateQuestCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            return response;
        }
    }
}