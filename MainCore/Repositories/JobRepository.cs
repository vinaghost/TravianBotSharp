using FluentResults;
using Humanizer;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Extensions;
using MainCore.Common.Models;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using MainCore.UI.Models.Output;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class JobRepository : IJobRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        private readonly Dictionary<Type, JobTypeEnums> _jobTypes = new()
        {
            { typeof(NormalBuildPlan),JobTypeEnums.NormalBuild  },
            { typeof(ResourceBuildPlan),JobTypeEnums.ResourceBuild },
        };

        public JobRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void AddToTop<T>(VillageId villageId, T content)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .ExecuteUpdate(x =>
                   x.SetProperty(x => x.Position, x => x.Position + 1));

            var job = new Job()
            {
                Position = 0,
                VillageId = villageId.Value,
                Type = _jobTypes[typeof(T)],
                Content = JsonSerializer.Serialize(content),
            };
            context.Add(job);
            context.SaveChanges();
        }

        public void Add<T>(VillageId villageId, T content)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Count();

            var job = new Job()
            {
                Position = count,
                VillageId = villageId.Value,
                Type = _jobTypes[typeof(T)],
                Content = JsonSerializer.Serialize(content),
            };

            context.Add(job);
            context.SaveChanges();
        }

        public void AddRange(VillageId villageId, IEnumerable<JobDto> jobDtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var count = context.Jobs
               .Where(x => x.VillageId == villageId.Value)
               .Count();

            var jobs = new List<Job>();
            foreach (var jobDto in jobDtos)
            {
                var job = new Job()
                {
                    Position = count,
                    VillageId = villageId.Value,
                    Type = jobDto.Type,
                    Content = jobDto.Content,
                };

                jobs.Add(job);
            }

            context.AddRange(jobs);
            context.SaveChanges();
        }

        public int CountBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var types = new List<JobTypeEnums>()
            {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            var count = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => types.Contains(x.Type))
                .Count();
            return count;
        }

        public JobDto GetResourceBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => resourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();

            var resourceBuildJob = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.ResourceBuild)
                .ToDto()
                .FirstOrDefault();
            if (job is null) return resourceBuildJob;
            if (resourceBuildJob is null) return job;
            if (job.Position < resourceBuildJob.Position) return job;
            return resourceBuildJob;
        }

        public JobDto GetInfrastructureBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var resourceTypes = new List<BuildingEnums>()
            {
                BuildingEnums.Woodcutter,
                BuildingEnums.ClayPit,
                BuildingEnums.IronMine,
                BuildingEnums.Cropland
            };

            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == JobTypeEnums.NormalBuild)
                .ToDto()
                .AsEnumerable()
                .Select(x => new
                {
                    Job = x,
                    Content = JsonSerializer.Deserialize<NormalBuildPlan>(x.Content)
                })
                .Where(x => !resourceTypes.Contains(x.Content.Type))
                .Select(x => x.Job)
                .OrderBy(x => x.Position)
                .FirstOrDefault();
            return job;
        }

        public List<JobDto> GetJobs(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var jobs = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .ToList();
            return jobs;
        }

        public JobDto GetBuildingJob(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var types = new List<JobTypeEnums>()
            {
                JobTypeEnums.NormalBuild,
                JobTypeEnums.ResourceBuild
            };
            var job = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => types.Contains(x.Type))
                .OrderBy(x => x.Position)
                .ToDto()
                .FirstOrDefault();
            return job;
        }

        public void Delete(JobId jobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var job = context.Jobs
                .Where(x => x.Id == jobId.Value)
                .Select(x => new
                {
                    x.VillageId,
                    x.Position
                })
                .FirstOrDefault();

            if (job is null) return;

            context.Jobs
                .Where(x => x.Id == jobId.Value)
                .ExecuteDelete();

            context.Jobs
                .Where(x => x.VillageId == job.VillageId)
                .Where(x => x.Position > job.Position)
                .ExecuteUpdate(x => x.SetProperty(x => x.Position, x => x.Position - 1));
        }

        public void Delete(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .ExecuteDelete();
        }

        public void Move(JobId oldJobId, JobId newJobId)
        {
            using var context = _contextFactory.CreateDbContext();

            var jobIds = new List<int>() { oldJobId.Value, newJobId.Value };

            var jobs = context.Jobs
                .Where(x => jobIds.Contains(x.Id))
                .ToList();

            if (jobs.Count != 2) return;

            (jobs[0].Position, jobs[1].Position) = (jobs[1].Position, jobs[0].Position);
            context.UpdateRange(jobs);
            context.SaveChanges();
        }

        public List<ListBoxItem> GetItems(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();

            var items = context.Jobs
                .Where(x => x.VillageId == villageId.Value)
                .OrderBy(x => x.Position)
                .ToDto()
                .AsEnumerable()
                .Select(x => new ListBoxItem()
                {
                    Id = x.Id.Value,
                    Content = GetContent(x),
                })
                .ToList();

            return items;
        }

        private static string GetContent(JobDto job)
        {
            switch (job.Type)
            {
                case JobTypeEnums.NormalBuild:
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
                        return $"Build {plan.Type.Humanize()} to level {plan.Level} at location {plan.Location}";
                    }
                case JobTypeEnums.ResourceBuild:
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content);
                        return $"Build {plan.Plan.Humanize()} to level {plan.Level}";
                    }
                default:
                    return job.Content;
            }
        }

        public bool JobComplete(VillageId villageId, JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return false;
            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);

            using var context = _contextFactory.CreateDbContext();

            var queueBuilding = context.QueueBuildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .OrderByDescending(x => x.Level)
                .Select(x => x.Level)
                .FirstOrDefault();

            if (queueBuilding >= plan.Level) return true;

            var villageBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .Select(x => x.Level)
                .FirstOrDefault();
            if (villageBuilding >= plan.Level) return true;

            return false;
        }

        public Result JobValid(VillageId villageId, JobDto job)
        {
            if (job.Type == JobTypeEnums.ResourceBuild) return Result.Ok();
            var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content);
            if (plan.Type.IsResourceField()) return Result.Ok();
            using var context = _contextFactory.CreateDbContext();

            var currentBuilding = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Location == plan.Location)
                .FirstOrDefault();

            if (currentBuilding is not null && currentBuilding.Type == plan.Type) return Result.Ok();

            var prerequisiteBuildings = plan.Type.GetPrerequisiteBuildings();

            foreach (var prerequisiteBuilding in prerequisiteBuildings)
            {
                var vaild = context.Buildings
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Type == prerequisiteBuilding.Type)
                    .Any(x => x.Level >= prerequisiteBuilding.Level);
                if (!vaild) return Result.Fail(BuildingQueue.NotEnoughPrerequisiteBuilding(plan.Type, prerequisiteBuilding.Type, prerequisiteBuilding.Level));
            }

            if (!plan.Type.IsMultipleBuilding()) return Result.Ok();

            var building = context.Buildings
                .Where(x => x.VillageId == villageId.Value)
                .Where(x => x.Type == plan.Type)
                .OrderByDescending(x => x.Level)
                .FirstOrDefault();

            if (building is null) return Result.Ok();

            if (building.Level == building.Type.GetMaxLevel()) Result.Ok();

            return Result.Fail(BuildingQueue.NotEnoughPrerequisiteBuilding(building.Type, building.Level)); ;
        }
    }
}