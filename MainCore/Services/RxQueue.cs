namespace MainCore.Services
{
    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    [RegisterSingleton<IRxQueue, RxQueue>]
    public class RxQueue : IRxQueue
    {
        private readonly Signal<INotification> _notifications = new Signal<INotification>();
        private readonly ConnectableSignal<INotification> _connectableObservable;

        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public RxQueue(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;

            _connectableObservable = _notifications.ObserveOn(RxSchedulers.TaskpoolScheduler).Publish();
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
            using var context = _contextFactory.CreateDbContext();
            var taskManager = Locator.Current.GetService<ITaskManager>()!;

            taskManager.Add(new LoginTask.Task(accountId), first: true);

            var workTime = context.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            var sleepTask = new SleepTask.Task(accountId)
            {
                ExecuteAt = DateTime.Now.AddMinutes(workTime)
            };
            taskManager.AddOrUpdate(sleepTask);

            var startAdventureTask = new StartAdventureTask.Task(accountId);
            if (startAdventureTask.CanStart(context) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
            {
                taskManager.Add(startAdventureTask);
            }
            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))
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
            var hasBuildJobVillages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Jobs.Any(x => _jobTypes.Contains(x.Type)))
                .Select(x => new VillageId(x.Id))
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

        private static readonly List<JobTypeEnums> _jobTypes = new() {
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        };

        private void VillageTaskAddedHandler(VillageTaskAdded notification)
        {
            using var context = _contextFactory.CreateDbContext();
            notification.Task.SetVillageName(context);
        }

        public void RegisterHandler<T>(Action<T> handleAction) where T : INotification
        {
            _connectableObservable.OfType<T>().Subscribe(handleAction);
        }

        public void RegisterCommand<T>(ReactiveCommand<T, RxVoid> command) where T : INotification
        {
            GetObservable<T>().InvokeCommand(command);
        }

        public IObservable<T> GetObservable<T>() where T : INotification
        {
            return _connectableObservable.OfType<T>();
        }
    }
}