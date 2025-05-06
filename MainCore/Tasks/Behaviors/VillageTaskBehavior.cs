using MainCore.Commands.Checks;
using MainCore.Tasks.Base;

namespace MainCore.Tasks.Behaviors
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
        private readonly CheckQuestCommand.Handler _checkQuestCommand;

        public VillageTaskBehavior(SwitchVillageCommand.Handler switchVillageCommand, UpdateStorageCommand.Handler updateStorageCommand, ToDorfCommand.Handler toDorfCommand, UpdateBuildingCommand.Handler updateBuildingCommand, CheckQuestCommand.Handler checkQuestCommand)
        {
            _switchVillageCommand = switchVillageCommand;
            _updateStorageCommand = updateStorageCommand;
            _toDorfCommand = toDorfCommand;
            _updateBuildingCommand = updateBuildingCommand;
            _checkQuestCommand = checkQuestCommand;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;

            Result result;

            result = await _switchVillageCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            await _updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            var response = await Next(request, cancellationToken);

            result = await _toDorfCommand.HandleAsync(new(accountId, 0), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            result = await _updateBuildingCommand.HandleAsync(new(accountId, villageId), cancellationToken);
            if (result.IsFailed) return (TResponse)result;

            await _updateStorageCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            await _checkQuestCommand.HandleAsync(new(accountId, villageId), cancellationToken);

            return response;
        }
    }
}