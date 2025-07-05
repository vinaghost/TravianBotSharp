using MainCore.Constraints;
using MainCore.Specifications;

namespace MainCore.Notifications.Trigger
{
    [Handler]
    public static partial class UpdateBuildingTaskTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountConstraint notification,
            ITaskManager taskManager,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountId = notification.AccountId;

            var settingEnable = context.BooleanByName(accountId, AccountSettingEnums.EnableAutoLoadVillageBuilding);
            if (!settingEnable) return;

            var missingBuildingVillagesSpec = new MissingBuildingVillagesSpec(accountId);

            var villages = context.Villages
                .WithSpecification(missingBuildingVillagesSpec)
                .ToList();

            foreach (var village in villages)
            {
                var villageName = context.GetVillageName(village);
                taskManager.AddOrUpdate<UpdateBuildingTask.Task>(new(accountId, village, villageName));
            }
        }
    }
}