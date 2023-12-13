using MainCore.DTO;
using MainCore.Entities;
using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    public interface IJobRepository
    {
        void Add<T>(VillageId villageId, T content);

        void AddRange(VillageId villageId, IEnumerable<JobDto> jobDtos);

        void AddToTop<T>(VillageId villageId, T content);

        int CountBuildingJob(VillageId villageId);

        void Delete(VillageId villageId);

        void Delete(JobId jobId);

        JobDto GetBuildingJob(VillageId villageId);

        JobDto GetInfrastructureBuildingJob(VillageId villageId);

        List<ListBoxItem> GetItems(VillageId villageId);

        List<JobDto> GetJobs(VillageId villageId);

        JobDto GetResourceBuildingJob(VillageId villageId);

        bool JobComplete(VillageId villageId, JobDto job);

        void Move(JobId oldJobId, JobId newJobId);
    }
}