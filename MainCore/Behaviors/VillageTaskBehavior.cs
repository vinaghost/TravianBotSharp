using MainCore.Tasks.Base;

namespace MainCore.Behaviors
{
    public class VillageTaskBehavior<TRequest, TResponse>
            : Behavior<TRequest, TResponse>
                where TRequest : VillageTask
                where TResponse : Result
    {
        private readonly SwitchVillageCommand.Handler _switchVillageCommand;
        private readonly UpdateStorageCommand.Handler _updateStorageCommand;
        private readonly ToDorfCommand.Handler _toDorfCommand;
        private readonly UpdateBuildingCommand.Handler _updateBuildingCommand;
        private readonly UpdateQuestCommand.Handler _updateQuestCommand;

        public VillageTaskBehavior(SwitchVillageCommand.Handler switchVillageCommand, UpdateStorageCommand.Handler updateStorageCommand, ToDorfCommand.Handler toDorfCommand, UpdateBuildingCommand.Handler updateBuildingCommand, UpdateQuestCommand.Handler updateQuestCommand)
        {
            _switchVillageCommand = switchVillageCommand;
            _updateStorageCommand = updateStorageCommand;
            _toDorfCommand = toDorfCommand;
            _updateBuildingCommand = updateBuildingCommand;
            _updateQuestCommand = updateQuestCommand;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;

            var (_, isFailed, errors) = await _switchVillageCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            await _updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            (_, isFailed, errors) = await _updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            var response = await Next(request, cancellationToken);

            if (response.IsFailed && !response.HasError<Skip>()) return response;

            (_, isFailed, errors) = await _toDorfCommand.HandleAsync(new(0), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            (_, isFailed, errors) = await _updateBuildingCommand.HandleAsync(new(villageId), cancellationToken);
            if (isFailed) return (TResponse)Result.Fail(errors);

            await _updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            await _updateQuestCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            return response;
        }
    }
}