using System.Text.Json;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class AddCroplandCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            GetLayoutBuildingsCommand.Handler getLayoutBuildingsQuery,
            AddJobCommand.Handler addJobCommand,
            IRxQueue rxQueue,
            CancellationToken cancellationToken)
        {
            var villageId = command.VillageId;

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
            var cropLandJob = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.NormalBuild,
                Content = JsonSerializer.Serialize(cropLandPlan),
            };

            await addJobCommand.HandleAsync(new(villageId, cropLandJob, true), cancellationToken);
            rxQueue.Enqueue(new JobsModified(villageId));
        }
    }
}