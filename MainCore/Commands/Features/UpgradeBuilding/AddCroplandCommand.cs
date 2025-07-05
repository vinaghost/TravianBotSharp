using MainCore.Constraints;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class AddCroplandCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            GetLayoutBuildingsQuery.Handler getLayoutBuildingsQuery,
            AddJobCommand.Handler addJobCommand,
            IRxQueue rxQueue,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;

            var buildings = await getLayoutBuildingsQuery.HandleAsync(new(villageId, true), cancellationToken);

            var cropland = buildings
                .Where(x => x.Type == BuildingEnums.Cropland)
                .OrderBy(x => x.Level)
                .First();

            var cropLandPlan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };
            await addJobCommand.HandleAsync(new(villageId, cropLandPlan.ToJob(), true), cancellationToken);
            rxQueue.Enqueue(new JobsModified(villageId));
        }
    }
}