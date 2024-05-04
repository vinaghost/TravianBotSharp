using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerRefreshVillageTask : INotificationHandler<VillageSettingUpdated>, INotificationHandler<AccountInit>
    {
        private readonly ITaskManager _taskManager;
        private readonly IVillageRepository _villageRepository;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public TriggerRefreshVillageTask(ITaskManager taskManager, IVillageRepository villageRepository, IVillageSettingRepository villageSettingRepository)
        {
            _taskManager = taskManager;
            _villageRepository = villageRepository;
            _villageSettingRepository = villageSettingRepository;
        }

        public async Task Handle(VillageSettingUpdated notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var villageId = notification.VillageId;
            await Trigger(accountId, villageId);
        }

        public async Task Handle(AccountInit notification, CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;

            var villages = _villageRepository.Get(accountId);
            foreach (var village in villages)
            {
                await Trigger(accountId, village);
            }
        }

        private async Task Trigger(AccountId accountId, VillageId villageId)
        {
            var autoRefreshEnable = new GetVillageSetting().GetBooleanByName(villageId, VillageSettingEnums.AutoRefreshEnable);
            if (autoRefreshEnable)
            {
                if (_taskManager.IsExist<UpdateVillageTask>(accountId, villageId)) return;
                await _taskManager.Add<UpdateVillageTask>(accountId, villageId);
            }
            else
            {
                var task = _taskManager.Get<UpdateVillageTask>(accountId, villageId);
                await _taskManager.Remove(accountId, task);
            }
        }
    }
}