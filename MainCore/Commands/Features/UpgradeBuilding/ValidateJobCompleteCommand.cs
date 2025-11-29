namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ValidatePlanCompleteCommand
    {
        public sealed record Command(VillageId VillageId, NormalBuildPlan Plan) : IVillageCommand;

        private static async ValueTask<Result<bool>> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, plan) = command;

            var (buildings, queueBuildings) = context.GetBuildings(villageId);

            var oldBuilding = buildings
              .FirstOrDefault(x => x.Location == plan.Location);

            if (oldBuilding is not null && oldBuilding.Type == plan.Type)
            {
                if (oldBuilding.Level >= plan.Level) return false;

                var queueBuilding = queueBuildings
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

                if (queueBuilding >= plan.Level) return false;
                return true;
            }

            var errors = new List<IError>();
            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = buildings
                   .Any(x => x.Type == prerequisiteBuilding.Type && x.Level >= prerequisiteBuilding.Level);

                if (!vaild)
                {
                    errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(prerequisiteBuilding.Type, prerequisiteBuilding.Level));
                    var queueBuilding = queueBuildings.Find(x => x.Type == prerequisiteBuilding.Type && x.Level == prerequisiteBuilding.Level);
                    if (queueBuilding is not null)
                    {
                        errors.Add(NextExecuteError.PrerequisiteBuildingInQueue(prerequisiteBuilding.Type, prerequisiteBuilding.Level, queueBuilding.CompleteTime));
                    }
                }
            }

            if (!plan.Type.IsMultipleBuilding()) return errors.Count == 0 ? true : Result.Fail(errors);

            var firstBuilding = buildings
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (firstBuilding is null) return errors.Count == 0 ? true : Result.Fail(errors);
            if (firstBuilding.Location == plan.Location) return errors.Count == 0 ? true : Result.Fail(errors);
            if (firstBuilding.Level == firstBuilding.Type.GetMaxLevel()) return errors.Count == 0 ? true : Result.Fail(errors);

            errors.Add(UpgradeBuildingError.PrerequisiteBuildingMissing(firstBuilding.Type, firstBuilding.Level));
            var prerequisiteBuildingUndercontruction = queueBuildings.Find(x => x.Type == firstBuilding.Type && x.Level == firstBuilding.Level);
            if (prerequisiteBuildingUndercontruction is not null)
            {
                errors.Add(NextExecuteError.PrerequisiteBuildingInQueue(firstBuilding.Type, firstBuilding.Level, prerequisiteBuildingUndercontruction.CompleteTime));
            }

            return Result.Fail(errors);
        }

        private static (List<Building>, List<QueueBuilding>) GetBuildings(this AppDbContext context, VillageId villageId)
        {
            var completeQueueBuildings = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.CompleteTime < DateTime.Now)
                .OrderBy(x => x.Level)
                .ToList();

            if (completeQueueBuildings.Count > 0)
            {
                foreach (var completeQueueBuilding in completeQueueBuildings)
                {
                    if (completeQueueBuilding.Location == -1) continue;

                    var building = context.Buildings
                        .Where(x => x.VillageId == villageId.Value)
                        .Where(x => x.Location == completeQueueBuilding.Location)
                        .FirstOrDefault();
                    if (building is null) continue;

                    building.Level = completeQueueBuilding.Level;
                    context.Remove(completeQueueBuilding);
                }
                context.SaveChanges();
            }

            var buildings = context.Buildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .ToList();

            var queueBuildings = context.QueueBuildings
                .AsNoTracking()
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.CompleteTime)
                .ToList();

            return (buildings, queueBuildings);
        }
    }
}