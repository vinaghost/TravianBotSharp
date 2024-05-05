using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerBuildingUpdateTask : INotificationHandler<VillageUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly IAccountSettingRepository _accountSettingRepository;
        private readonly IVillageRepository _villageRepository;

        public TriggerBuildingUpdateTask(ITaskManager taskManager, IAccountSettingRepository accountSettingRepository, IVillageRepository villageRepository)
        {
            _taskManager = taskManager;
            _accountSettingRepository = accountSettingRepository;
            _villageRepository = villageRepository;
        }

        public async Task Handle(VillageUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoLoadVillageBuilding = new GetAccountSetting().BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = _villageRepository.GetMissingBuildingVillages(accountId);

            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}