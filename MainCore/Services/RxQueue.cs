using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace MainCore.Services
{
    [RegisterSingleton<IRxQueue, RxQueue>]
    public class RxQueue : IRxQueue
    {
        private readonly Subject<INotification> _notifications = new Subject<INotification>();
        private readonly IConnectableObservable<INotification> _connectableObservable;

        private readonly ICustomServiceScopeFactory _serviceScopeFactory;

        public RxQueue(ICustomServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _connectableObservable = _notifications.ObserveOn(Scheduler.Default).Publish();
            _connectableObservable.Connect();
        }

        public void Enqueue(INotification notification)
        {
            _notifications.OnNext(notification);
        }

        public void Setup()
        {
            RegisterHandler<AccountInit>(AccountInitHandler);
            RegisterHandler<VillageTaskAdded>(VillageTaskAddedHandler);
        }

        private void AccountInitHandler(AccountInit notification)
        {
            var accountId = notification.AccountId;
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var taskManager = scope.ServiceProvider.GetRequiredService<ITaskManager>();
            taskManager.Add(new LoginTask.Task(accountId), first: true);
            var workTime = context.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            var sleepTask = new SleepTask.Task(accountId);
            sleepTask.ExecuteAt = DateTime.Now.AddMinutes(workTime);
            taskManager.AddOrUpdate<SleepTask.Task>(sleepTask);
            var startAdventureTask = new StartAdventureTask.Task(accountId);
            if (startAdventureTask.CanStart(context) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
            {
                taskManager.Add(startAdventureTask);
            }
            var villagesSpec = new VillagesSpec(accountId);
            var villages = context.Villages
                .WithSpecification(villagesSpec)
                .ToList();
            foreach (var village in villages)
            {
                var updateVillageTask = new UpdateVillageTask.Task(accountId, village);
                if (updateVillageTask.CanStart(context) && !taskManager.IsExist<UpdateVillageTask.Task>(accountId, village))
                {
                    taskManager.Add(updateVillageTask);
                }
                var trainTroopTask = new TrainTroopTask.Task(accountId, village);
                if (trainTroopTask.CanStart(context) && !taskManager.IsExist<TrainTroopTask.Task>(accountId, village))
                {
                    taskManager.Add(trainTroopTask);
                }
            }
            var hasBuildJobVillagesSpec = new HasBuildJobVillagesSpec(accountId);
            var hasBuildJobVillages = context.Villages
                .WithSpecification(hasBuildJobVillagesSpec)
                .ToList();
            foreach (var village in hasBuildJobVillages)
            {
                var upgradeBuildingTask = new UpgradeBuildingTask.Task(accountId, village);
                if (!taskManager.IsExist<UpgradeBuildingTask.Task>(accountId, village))
                {
                    taskManager.Add(upgradeBuildingTask);
                }
            }
        }

        private void VillageTaskAddedHandler(VillageTaskAdded notification)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            notification.Task.SetVillageName(context);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : INotification
        {
            _connectableObservable.OfType<T>().Subscribe(handleAction);
        }

        public void RegisterCommand<T>(ReactiveCommand<T, Unit> command) where T : INotification
        {
            GetObservable<T>().InvokeCommand(command);
        }

        public IObservable<T> GetObservable<T>() where T : INotification
        {
            return _connectableObservable.OfType<T>();
        }
    }
}