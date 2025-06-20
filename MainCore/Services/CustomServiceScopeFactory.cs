using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Services
{
    [RegisterSingleton<ICustomServiceScopeFactory, CustomServiceScopeFactory>]
    public class CustomServiceScopeFactory : ICustomServiceScopeFactory
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CustomServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public IServiceScope CreateScope()
        {
            return _serviceScopeFactory.CreateScope();
        }

        public IServiceScope CreateScope(AccountId accountId)
        {
            var scope = _serviceScopeFactory.CreateScope();
            var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var account = context.Accounts
               .Where(x => x.Id == accountId.Value)
               .Select(x => new
               {
                   x.Username,
                   x.Server,
               })
               .FirstOrDefault();

            if (account is null)
            {
                throw new InvalidOperationException($"Account with ID {accountId.Value} not found.");
            }

            var uri = new Uri(account.Server);

            dataService.AccountId = accountId;
            dataService.AccountData = $"{account.Username}_{uri.Host}";
            return scope;
        }
    }

    public static class CustomServiceScopeExtension
    {
        private static IHandler<T, Result> GetHandler<T>(this IServiceScope scope)
        {
            return scope.ServiceProvider.GetRequiredService<IHandler<T, Result>>();
        }

        public static async Task<Result> Execute(this IServiceScope scope, BaseTask task, CancellationToken cancellationToken)
        {
            switch (task)
            {
                case ClaimQuestTask.Task claimQuestTask:
                    var claimQuestTaskHandler = scope.GetHandler<ClaimQuestTask.Task>();
                    return await claimQuestTaskHandler.HandleAsync(claimQuestTask, cancellationToken);

                case CompleteImmediatelyTask.Task completeImmediatelyTask:
                    var completeImmediatelyTaskHandler = scope.GetHandler<CompleteImmediatelyTask.Task>();
                    return await completeImmediatelyTaskHandler.HandleAsync(completeImmediatelyTask, cancellationToken);

                case LoginTask.Task loginTask:
                    var loginTaskHandler = scope.GetHandler<LoginTask.Task>();
                    return await loginTaskHandler.HandleAsync(loginTask, cancellationToken);

                case NpcTask.Task npcTask:
                    var npcTaskHandler = scope.GetHandler<NpcTask.Task>();
                    return await npcTaskHandler.HandleAsync(npcTask, cancellationToken);

                case SleepTask.Task sleepTask:
                    var sleepTaskHandler = scope.GetHandler<SleepTask.Task>();
                    return await sleepTaskHandler.HandleAsync(sleepTask, cancellationToken);

                case StartAdventureTask.Task startAdventureTask:
                    var startAdventureTaskHandler = scope.GetHandler<StartAdventureTask.Task>();
                    return await startAdventureTaskHandler.HandleAsync(startAdventureTask, cancellationToken);

                case StartFarmListTask.Task startFarmListTask:
                    var startFarmListTaskHandler = scope.GetHandler<StartFarmListTask.Task>();
                    return await startFarmListTaskHandler.HandleAsync(startFarmListTask, cancellationToken);

                case TrainTroopTask.Task trainTroopTask:
                    var trainTroopTaskHandler = scope.GetHandler<TrainTroopTask.Task>();
                    return await trainTroopTaskHandler.HandleAsync(trainTroopTask, cancellationToken);

                case UpdateBuildingTask.Task updateBuildingTask:
                    var updateBuildingTaskHandler = scope.GetHandler<UpdateBuildingTask.Task>();
                    return await updateBuildingTaskHandler.HandleAsync(updateBuildingTask, cancellationToken);

                case UpdateFarmListTask.Task updateFarmListTask:
                    var updateFarmListTaskHandler = scope.GetHandler<UpdateFarmListTask.Task>();
                    return await updateFarmListTaskHandler.HandleAsync(updateFarmListTask, cancellationToken);

                case UpdateVillageTask.Task updateVillageTask:
                    var updateVillageTaskHandler = scope.GetHandler<UpdateVillageTask.Task>();
                    return await updateVillageTaskHandler.HandleAsync(updateVillageTask, cancellationToken);

                case UpgradeBuildingTask.Task upgradeBuildingTask:
                    var upgradeBuildingTaskHandler = scope.GetHandler<UpgradeBuildingTask.Task>();
                    return await upgradeBuildingTaskHandler.HandleAsync(upgradeBuildingTask, cancellationToken);

                default:
                    throw new NotImplementedException($"Task {task.GetType().Name} is not implemented");
            }
        }
    }
}