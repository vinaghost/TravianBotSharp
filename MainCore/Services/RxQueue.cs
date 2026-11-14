using System.Linq;

namespace MainCore.Services
{
    using System.Collections.Frozen;

    using MainCore.Features.Login;
    using MainCore.Features.Sleep;
    using MainCore.Features.StartAdventure;
    using MainCore.Features.TrainTroop;
    using MainCore.Features.Update;
    using MainCore.Features.UpgradeBuilding;
    using MainCore.Infrasturecture.Extensions;
    using MainCore.Shared.Constraints;
    using MainCore.Shared.Enums;
    using MainCore.Shared.Notifications;
    using MainCore.Shared.Tasks;

    using ReactiveUI.Primitives;
    using ReactiveUI.Primitives.Signals;

    using Splat;

    [RegisterSingleton<RxQueue>]
    public class RxQueue
    {
        private readonly Signal<INotification> _notifications = new();
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
            var taskManager = Locator.Current.GetService<TaskManager>()!;

            taskManager.Add(new LoginTask.Task(accountId), first: true);

            var workTime = context.ByName(accountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            var sleepTask = new SleepTask.Task(accountId)
            {
                ExecuteAt = DateTime.Now.AddMinutes(workTime)
            };
            taskManager.AddOrUpdate(sleepTask);

            if (context.IsStartAdventureEnable(accountId) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
            {
                taskManager.Add(new StartAdventureTask.Task(accountId));
            }

            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Select(x => new VillageId(x.Id))
                .ToList();

            foreach (var village in villages)
            {
                if (context.IsRefreshEnable(village) && !taskManager.IsExist<UpdateVillageTask.Task>(accountId, village))
                {
                    taskManager.Add(new UpdateVillageTask.Task(accountId, village));
                }

                if (context.IsTrainTroopEnable(village) && !taskManager.IsExist<TrainTroopTask.Task>(accountId, village))
                {
                    taskManager.Add(new TrainTroopTask.Task(accountId, village));
                }
            }

            var hasBuildJobVillages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Where(x => x.Jobs.Any(j => _jobTypes.Contains(j.Type)))
                .Select(x => new VillageId(x.Id))
                .ToList();
            foreach (var village in hasBuildJobVillages.Where(village => !taskManager.IsExist<UpgradeBuildingTask.Task>(accountId, village)))
            {
                taskManager.Add(new UpgradeBuildingTask.Task(accountId, village));
            }
        }

        private static readonly List<JobTypeEnums> _jobTypes =
        [
            JobTypeEnums.NormalBuild,
            JobTypeEnums.ResourceBuild
        ];

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