using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerBuildingUpdateTask : INotificationHandler<VillageUpdated>
    {
        private readonly ITaskManager _taskManager;
        private readonly GetVillage _getVillage;
        private readonly GetSetting _getSetting;

        public TriggerBuildingUpdateTask(ITaskManager taskManager, GetVillage getVillage, GetSetting getSetting)
        {
            _taskManager = taskManager;
            _getVillage = getVillage;
            _getSetting = getSetting;
        }

        public async Task Handle(VillageUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoLoadVillageBuilding = _getSetting.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = _getVillage.Missing(accountId);

            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}