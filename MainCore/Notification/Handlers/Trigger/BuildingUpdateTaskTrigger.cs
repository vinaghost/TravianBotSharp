using MainCore.Tasks;

namespace MainCore.Notification.Handlers.Trigger
{
    [Handler]
    public static partial class BuildingUpdateTaskTrigger
    {
        private static async ValueTask HandleAsync(
            ByAccountIdBase notification,
            ITaskManager taskManager, GetVillage getVillage, IGetSetting getSetting,
            CancellationToken cancellationToken)
        {
            var accountId = notification.AccountId;
            var autoLoadVillageBuilding = getSetting.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!autoLoadVillageBuilding) return;

            var villages = getVillage.Missing(accountId);

            foreach (var village in villages)
            {
                await taskManager.AddOrUpdate<UpdateBuildingTask>(accountId, village);
            }
        }
    }
}