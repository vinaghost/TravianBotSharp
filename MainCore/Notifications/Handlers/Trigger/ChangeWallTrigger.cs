using MainCore.Constraints;

namespace MainCore.Notifications.Handlers.Trigger
{
    [Handler]
    public static partial class ChangeWallTrigger
    {
        private static async ValueTask HandleAsync(
            IAccountVillageConstraint notification,
            AppDbContext context,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var villageId = notification.VillageId;

            var wallBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == 40)
                .FirstOrDefault();
            if (wallBuilding is null) return;

            var tribe = (TribeEnums)context.VillagesSetting
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Setting == VillageSettingEnums.Tribe)
                .Select(x => x.Value)
                .FirstOrDefault();

            var wall = tribe.GetWall();
            if (wallBuilding.Type == wall) return;

            wallBuilding.Type = wall;
            context.Update(wallBuilding);
            context.SaveChanges();
        }
    }
}