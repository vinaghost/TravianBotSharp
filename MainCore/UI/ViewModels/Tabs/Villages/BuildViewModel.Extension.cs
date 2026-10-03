using MainCore.Infrasturecture.Extensions;

namespace MainCore.UI.ViewModels.Tabs.Villages
{
    public static class BuildViewModelExtension
    {
        public static int SwapJob(this AppDbContext context, JobId jobId, MoveEnums move)
        {
            var job = context.Jobs
                .FirstOrDefault(x => x.Id == jobId.Value);

            if (job is null) return -1;

            var currentPosition = job.Position;
            Job? targetJob;

            switch (move)
            {
                case MoveEnums.Up:
                    if (currentPosition == 0) return currentPosition;
                    targetJob = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .FirstOrDefault(x => x.Position == currentPosition - 1);
                    break;

                case MoveEnums.Down:
                    var count = context.Jobs
                        .Count(x => x.VillageId == job.VillageId);
                    if (currentPosition == count - 1) return currentPosition;
                    targetJob = context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .FirstOrDefault(x => x.Position == currentPosition + 1);
                    break;

                default:
                    return currentPosition;
            }
            if (targetJob is null) return currentPosition;

            (targetJob.Position, job.Position) = (job.Position, targetJob.Position);

            context.Update(job);
            context.Update(targetJob);
            context.SaveChanges();

            return job.Position;
        }

        public static int MoveJob(this AppDbContext context, JobId jobId, MoveEnums move)
        {
            var job = context.Jobs
                    .FirstOrDefault(x => x.Id == jobId.Value);

            if (job is null) return -1;

            var currentPosition = job.Position;

            switch (move)
            {
                case MoveEnums.Top:
                    if (currentPosition == 0) return currentPosition;
                    context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position < currentPosition)
                        .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position + 1));

                    job.Position = 0;
                    break;

                case MoveEnums.Bottom:
                    var count = context.Jobs
                        .Count(x => x.VillageId == job.VillageId);
                    if (currentPosition == count - 1) return currentPosition;
                    context.Jobs
                        .Where(x => x.VillageId == job.VillageId)
                        .Where(x => x.Position > currentPosition)
                        .ExecuteUpdate(x => x.SetProperty(y => y.Position, y => y.Position - 1));

                    job.Position = count - 1;
                    break;

                default:
                    return currentPosition;
            }

            context.Update(job);
            context.SaveChanges();

            return job.Position;
        }

        public static void Upgrade(this AppDbContext context, VillageId villageId, int location, bool isMaxLevel)
        {
            var buildings = context.GetLayoutBuildings(villageId);
            var building = buildings.Find(x => x.Location == location);

            if (building is null) return;
            if (building.Type == BuildingEnums.Site) return;

            var level = 0;

            if (isMaxLevel)
            {
                level = building.Type.GetMaxLevel();
            }
            else
            {
                level = building.Level + 1;
            }

            var plan = new NormalBuildPlan()
            {
                Location = location,
                Type = building.Type,
                Level = level,
            };

            context.AddJob(villageId, plan);
        }

        public static Result CheckRequirements(this BuildingEnums buildingType, List<BuildingItem> buildings)
        {
            var prerequisiteBuildings = buildingType.GetPrerequisiteBuildings();
            if (prerequisiteBuildings.Count == 0) return Result.Ok();
            IList<Error> errors = [];
            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var valid = buildings
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);

                if (!valid) errors.Add(new Error($"Required {prerequisiteBuilding}"));
            }
            return Result.FailIfNotEmpty(errors);
        }

        public static void FixLocation(this NormalBuildPlan plan, List<BuildingItem> buildings)
        {
            if (plan.Type.IsWall())
            {
                plan.Location = 40;
                return;
            }
            if (plan.Type.IsMultipleBuilding())
            {
                var sameTypeBuildings = buildings.Where(x => x.Type == plan.Type);
                if (!sameTypeBuildings.Any()) return;
                if (sameTypeBuildings.Any(x => x.Location == plan.Location)) return;
                var largestLevelBuilding = sameTypeBuildings.MaxBy(x => x.Level)!;
                if (largestLevelBuilding.Level == plan.Type.GetMaxLevel()) return;
                plan.Location = largestLevelBuilding.Location;
                return;
            }

            if (plan.Type.IsResourceField())
            {
                var field = buildings.First(x => x.Location == plan.Location);
                if (plan.Type == field.Type) return;
                plan.Type = field.Type;
                return;
            }

            var building = buildings.Find(x => x.Type == plan.Type);
            if (building is null) return;
            if (plan.Location == building.Location) return;
            plan.Location = building.Location;
        }
    }
}