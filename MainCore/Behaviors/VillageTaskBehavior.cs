using MainCore.Commands.Navigate;
using MainCore.Commands.Update;
using MainCore.Tasks.Base;

namespace MainCore.Behaviors
{
    public class VillageTaskBehavior<TRequest, TResponse>(
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

            var (_, isFailed, errors) = await switchVillageCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            var response = await Next(request, cancellationToken);

            if (response.IsFailed && !response.HasError<Skip>()) return response;

            (_, isFailed, errors) = await toDorfCommand.HandleAsync(new(0), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            (_, isFailed, errors) = await updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            await updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            await updateQuestCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            return response;
        }
    }
}