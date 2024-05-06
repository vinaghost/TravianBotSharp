using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    public class TriggerBuildingUpdateTask : INotificationHandler<VillageUpdated>
    {
        private readonly ITaskManager _taskManager;

        public TriggerBuildingUpdateTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        public async Task Handle(VillageUpdated notification, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;
            await Trigger(accountId);
        }

        private async Task Trigger(AccountId accountId)
        {
            var autoLoadVillageBuilding = new GetSetting().BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = new GetVillage().Missing(accountId);

            foreach (var village in villages)
            {
                await _taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}