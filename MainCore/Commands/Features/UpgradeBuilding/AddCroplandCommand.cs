using MainCore.Constraints;

namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class AddCroplandCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            GetLowestBuildingQuery.Handler getLowestBuildingQuery,
            AddJobCommand.Handler addJobCommand,
            JobUpdated.Handler jobUpdated,
            CancellationToken cancellationToken)
        {
            var (accountId, villageId) = command;
            var cropland = await getLowestBuildingQuery.HandleAsync(new(villageId, BuildingEnums.Cropland), cancellationToken);

            var cropLandPlan = new NormalBuildPlan()
            {
                Location = cropland.Location,
                Type = cropland.Type,
                Level = cropland.Level + 1,
            };
            await addJobCommand.HandleAsync(new(villageId, cropLandPlan.ToJob(), true), cancellationToken);
            await jobUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
            return Result.Ok();
        }
    }
}