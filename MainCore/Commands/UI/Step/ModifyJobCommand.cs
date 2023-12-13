using FluentResults;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Common.MediatR;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using System.Text.Json;

namespace MainCore.Commands.UI.Step
{
    public class ModifyJobCommand : ByVillageIdBase, ICommand<List<JobDto>>
    {
        public List<JobDto> Jobs;

        public ModifyJobCommand(VillageId villageId, List<JobDto> jobs) : base(villageId)
        {
            Jobs = jobs;
        }
    }

    [RegisterAsTransient]
    public class ModifyJobCommandHandler : ICommandHandler<ModifyJobCommand, List<JobDto>>
    {
        private readonly UnitOfRepository _unitOfRepository;
        private readonly List<int> _excludedLocations = new() { 26, 39, 40 }; //main building, rallypoint and wall

        public ModifyJobCommandHandler(UnitOfRepository unitOfRepository)
        {
            _unitOfRepository = unitOfRepository;
        }

        public List<JobDto> Value { get; private set; }

        public async Task<Result> Handle(ModifyJobCommand command, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var modifiedJobs = GetModifiedJobs(command.VillageId, command.Jobs);
            Value = modifiedJobs.ToList();

            return Result.Ok();
        }

        private IEnumerable<JobDto> GetModifiedJobs(VillageId villageId, List<JobDto> jobs)
        {
            var buildings = _unitOfRepository.BuildingRepository.GetBuildingItems(villageId);

            var changedLocations = new Dictionary<int, int>();
            foreach (var job in jobs)
            {
                switch (job.Type)
                {
                    case JobTypeEnums.NormalBuild:
                        {
                            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                            if (plan.Type.IsResourceField()) continue;

                            Modify(buildings, plan, changedLocations);
                            job.Content = GetContent(plan);

                            if (IsValidPlan(buildings, plan)) yield return job;
                            continue;
                        }
                    case JobTypeEnums.ResourceBuild:
                        {
                            yield return job;
                            continue;
                        }
                    default:
                        continue;
                }
            }
        }

        private static string GetContent(NormalBuildPlan plan)
        {
            return JsonSerializer.Serialize(plan);
        }

        private static bool IsValidPlan(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var building = buildings.FirstOrDefault(x => x.Location == plan.Location);
            if (building is null) return false;
            if (building.JobLevel >= plan.Level) return false;

            if (building.Type == BuildingEnums.Site) building.Type = plan.Type;
            building.JobLevel = plan.Level;
            return true;
        }

        private void Modify(List<BuildingItem> buildings, NormalBuildPlan plan, Dictionary<int, int> changedLocations)
        {
            if (plan.Type.IsMultipleBuilding())
            {
                if (ModifyMultiple(buildings, plan)) return;
            }
            else
            {
                if (ModifySame(buildings, plan)) return;
            }

            if (_excludedLocations.Contains(plan.Location)) return;

            ModifyRandom(buildings, plan, changedLocations);
        }

        private static bool ModifyMultiple(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var largestLevelBuilding = buildings.Where(x => x.Type == plan.Type).OrderByDescending(x => x.Level).FirstOrDefault();
            if (largestLevelBuilding is not null && largestLevelBuilding.Type.GetMaxLevel() < largestLevelBuilding.Level)
            {
                plan.Location = largestLevelBuilding.Location;
                return true;
            }
            return false;
        }

        private static bool ModifySame(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var sameTypeBuilding = buildings.Where(x => x.Type == plan.Type).FirstOrDefault();
            if (sameTypeBuilding is not null)
            {
                if (sameTypeBuilding.Location != plan.Location)
                {
                    sameTypeBuilding.Location = plan.Location;
                }
                return true;
            }
            return false;
        }

        private void ModifyRandom(List<BuildingItem> buildings, NormalBuildPlan plan, Dictionary<int, int> changedLocations)
        {
            var freeLocations = buildings
               .Where(x => x.Type == BuildingEnums.Site)
               .Select(x => x.Location)
               .Where(x => !_excludedLocations.Contains(x))
               .ToList();

            var randomLocation = GetRandomLocation(freeLocations);
            changedLocations[plan.Location] = randomLocation;

            plan.Location = randomLocation;
        }

        private static int GetRandomLocation(List<int> freeLocations)
        {
            return freeLocations[Random.Shared.Next(0, freeLocations.Count - 1)];
        }
    }
}