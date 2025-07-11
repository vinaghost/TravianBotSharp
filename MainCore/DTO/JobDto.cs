#nullable disable

using Humanizer;
using System.Text.Json;

namespace MainCore.DTO
{
    public class JobDto
    {
        public JobId Id { get; set; }
        public int Position { get; set; }
        public JobTypeEnums Type { get; set; }
        public string Content { get; set; }

        public override string ToString()
        {
            return this.GetContent();
        }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class JobMapper
    {
        public static Job ToEntity(this JobDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Job.VillageId))]
        private static partial Job ToEntity(this JobDto dto);

        public static partial JobDto ToDto(this Job dto);

        public static partial IQueryable<JobDto> ToDto(this IQueryable<Job> entities);

        private static int ToInt(this JobId jobId) => jobId.Value;

        private static JobId ToJobId(this int value) => new(value);

        public static string GetContent(this JobDto job)
        {
            switch (job.Type)
            {
                case JobTypeEnums.NormalBuild:
                    {
                        var plan = JsonSerializer.Deserialize<NormalBuildPlan>(job.Content)!;
                        return $"Build {plan.Type.Humanize()} to level {plan.Level} at location {plan.Location}";
                    }
                case JobTypeEnums.ResourceBuild:
                    {
                        var plan = JsonSerializer.Deserialize<ResourceBuildPlan>(job.Content)!;
                        return $"Build {plan.Plan.Humanize()} to level {plan.Level}";
                    }
                default:
                    return job.Content;
            }
        }
    }
}