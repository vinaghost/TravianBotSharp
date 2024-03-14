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
        private static readonly List<int> _excludedLocations = new() { 26, 39, 40 }; //main building, rallypoint and wall

        private static readonly Dictionary<ResourcePlanEnums, List<BuildingEnums>> _fieldList = new()
        {
            {ResourcePlanEnums.AllResources, new(){
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland,}},
            {ResourcePlanEnums.ExcludeCrop, new() {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,}},
            {ResourcePlanEnums.OnlyCrop, new() {
                BuildingEnums.Cropland,}},
        };

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
            var buildings = _unitOfRepository.BuildingRepository.GetBuildings(villageId);

            var changedLocations = new Dictionary<int, int>();
            foreach (var job in jobs)
            {
                switch (job.Type)
                {
                    case JobTypeEnums.NormalBuild:
                        {
                            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

                            Modify(buildings, plan, changedLocations);
                            job.Content = GetContent(plan);

                            if (IsValidPlan(buildings, plan)) yield return job;
                            continue;
                        }
                    case JobTypeEnums.ResourceBuild:
                        {
                            var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                            if (IsValidPlan(buildings, plan)) yield return job;
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

        private static bool IsValidPlan(List<BuildingItem> buildings, ResourceBuildPlan plan)
        {
            var fieldTypes = _fieldList[plan.Plan];

            var fields = buildings
                .Where(x => fieldTypes.Contains(x.Type))
                .ToList();

            if (fields.TrueForAll(x => x.Level >= plan.Level)) return false;

            foreach (var field in fields)
            {
                if (field.Level < plan.Level) field.JobLevel = plan.Level;
            }
            return true;
        }

        private static bool IsValidPlan(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var building = buildings.FirstOrDefault(x => x.Location == plan.Location);
            if (building is null) return false;

            if (building.Type != BuildingEnums.Site)
            {
                if (building.Type != plan.Type) return false;
                if (building.Level >= plan.Level) return false;
            }
            else
            {
                building.Type = plan.Type;
            }

            building.JobLevel = plan.Level;
            return true;
        }

        private static void Modify(List<BuildingItem> buildings, NormalBuildPlan plan, Dictionary<int, int> changedLocations)
        {
            if (plan.Type.IsResourceField()) return;

            if (plan.Type.IsMultipleBuilding())
            {
                if (ModifyMultiple(buildings, plan)) return;
            }
            else
            {
                if (plan.Type.IsWall())
                {
                    var wall = buildings.FirstOrDefault(x => x.Location == 40);
                    if (plan.Type != wall.Type)
                    {
                        plan.Type = wall.Type;
                    }
                }

                if (ModifySame(buildings, plan)) return;
            }

            if (_excludedLocations.Contains(plan.Location)) return;

            ModifyRandom(buildings, plan, changedLocations);
        }

        private static bool ModifyMultiple(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var largestLevelBuilding = buildings
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();
            if (largestLevelBuilding is not null && largestLevelBuilding.Type.GetMaxLevel() > largestLevelBuilding.Level)
            {
                plan.Location = largestLevelBuilding.Location;
                return true;
            }
            return false;
        }

        private static bool ModifySame(List<BuildingItem> buildings, NormalBuildPlan plan)
        {
            var sameTypeBuilding = buildings
                .FirstOrDefault(x => x.Type == plan.Type);
            if (sameTypeBuilding is not null)
            {
                if (sameTypeBuilding.Location != plan.Location)
                {
                    plan.Location = sameTypeBuilding.Location;
                }
                return true;
            }
            return false;
        }

        private static void ModifyRandom(List<BuildingItem> buildings, NormalBuildPlan plan, Dictionary<int, int> changedLocations)
        {
            var freeLocations = buildings
               .Where(x => x.Type == BuildingEnums.Site)
               .Select(x => x.Location)
               .Where(x => !_excludedLocations.Contains(x))
               .ToList();

            if (!changedLocations.ContainsKey(plan.Location))
            {
                changedLocations[plan.Location] = GetRandomLocation(freeLocations);
            }

            plan.Location = changedLocations[plan.Location];
        }

        private static int GetRandomLocation(List<int> freeLocations)
        {
            return freeLocations[Random.Shared.Next(0, freeLocations.Count - 1)];
        }
    }
}