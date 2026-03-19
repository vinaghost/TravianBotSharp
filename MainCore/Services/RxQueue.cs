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

            var workStartHour = context.ByName(accountId, AccountSettingEnums.WorkStartHour);
            if (workStartHour < 0 || workStartHour > 23) workStartHour = 6;
            var workStartMinute = context.ByName(accountId, AccountSettingEnums.WorkStartMinute);
            if (workStartMinute < 0 || workStartMinute > 59) workStartMinute = 0;
            var workEndHour = context.ByName(accountId, AccountSettingEnums.WorkEndHour);
            if (workEndHour < 0 || workEndHour > 23) workEndHour = 22;
            var workEndMinute = context.ByName(accountId, AccountSettingEnums.WorkEndMinute);
            if (workEndMinute < 0 || workEndMinute > 59) workEndMinute = 0;
            var maxOffset = context.ByName(accountId, AccountSettingEnums.SleepRandomMinute);
            if (maxOffset < 0) maxOffset = 0;
            var random = new Random();
            var randomMinute = maxOffset == 0 ? 0 : random.Next(0, maxOffset);

            var now = DateTime.Now;
            DateTime sleepExecuteAt;

            var startToday = now.Date.AddHours(workStartHour).AddMinutes(workStartMinute);
            var endToday = now.Date.AddHours(workEndHour).AddMinutes(workEndMinute);
            
            // Detect overnight windows (e.g., 7:15 AM to 2:30 AM next day)
            bool windowCrossesMidnight = endToday < startToday;
            bool outsideWindow = windowCrossesMidnight 
                ? (now >= endToday && now < startToday)  // Outside if between end and start
                : (now < startToday || now >= endToday); // Outside if before start or at/after end
            
            DateTime nextStart = now < startToday ? startToday : startToday.AddDays(1);
            // helper used below when scheduling regular tasks
            DateTime WhenAllowed(DateTime original) => outsideWindow ? nextStart : original;

            if (outsideWindow)
            {
                // We're outside the window; schedule sleep for next start time
                sleepExecuteAt = nextStart;
            }
            else
            {
                // We're inside the window; schedule sleep for end of window
                sleepExecuteAt = endToday.AddMinutes(randomMinute);
            }

            var sleepTask = new SleepTask.Task(accountId);
            sleepTask.ExecuteAt = sleepExecuteAt;
            taskManager.AddOrUpdate<SleepTask.Task>(sleepTask);

            var startAdventureTask = new StartAdventureTask.Task(accountId);
            if (startAdventureTask.CanStart(context) && !taskManager.IsExist<StartAdventureTask.Task>(accountId))
            {
                startAdventureTask.ExecuteAt = WhenAllowed(now);
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
                    updateVillageTask.ExecuteAt = WhenAllowed(now);
                    taskManager.Add(updateVillageTask);
                }
                var trainTroopTask = new TrainTroopTask.Task(accountId, village);
                if (trainTroopTask.CanStart(context) && !taskManager.IsExist<TrainTroopTask.Task>(accountId, village))
                {
                    trainTroopTask.ExecuteAt = WhenAllowed(now);
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
                    upgradeBuildingTask.ExecuteAt = WhenAllowed(now);
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