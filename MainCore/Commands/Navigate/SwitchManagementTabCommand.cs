using MainCore.DTO;

namespace MainCore.Commands.Navigate
{
    [Handler]
    public static partial class SwitchManagementTabCommand
    {
        public sealed record Command(VillageId VillageId, int Location) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IBrowser browser,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            var (villageId, location) = command;
            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault(x => x.Location == location);

            if (building is null) return Result.Ok();

            Result result;
            if (building.Type == BuildingEnums.Site)
            {
                // Boþ arsa için hedef bina türünü job'dan alýyorum
                var targetBuildingType = GetTargetBuildingTypeFromJob(context, villageId, location);
                var tabIndex = targetBuildingType.GetBuildingsCategory();

                result = await SwitchTabCommand.SwitchTab(browser, tabIndex, cancellationToken);
                if (result.IsFailed) return result;
            }
            else
            {
                if (building.Level < 1) return Result.Ok();
                if (!building.Type.HasMultipleTabs()) return Result.Ok();

                result = await SwitchTabCommand.SwitchTab(browser, 0, cancellationToken);
                if (result.IsFailed) return result;
            }

            return Result.Ok();
        }

        private static BuildingEnums GetTargetBuildingTypeFromJob(AppDbContext context, VillageId villageId, int location)
        {
            // Önce QueueBuilding'den kontrol ediyorum - eðer zaten kuyruktaysa
            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == location)
                .OrderBy(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding is not null)
            {
                return queueBuilding.Type;
            }

            // Job'lardan hedef bina türünü alýyorum
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .Select(x => x.Content)
                .AsEnumerable()
                .Select(x => System.Text.Json.JsonSerializer.Deserialize<NormalBuildPlan>(x)!)
                .Where(x => x.Location == location)
                .OrderBy(x => x.Level)
                .FirstOrDefault();

            return job?.Type ?? BuildingEnums.Site;
        }
    }
}
