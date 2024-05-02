using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class JobDto
    {
        public JobId Id { get; set; }
        public int Position { get; set; }
        public JobTypeEnums Type { get; set; }
        public string Content { get; set; }
    }

    [Mapper]
    public static partial class JobMapper
    {
        public static Job ToEntity(this JobDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        public static partial JobDto ToDto(this Job dto);

        private static partial Job ToEntity(this JobDto dto);

        public static partial IQueryable<JobDto> ToDto(this IQueryable<Job> entities);

        private static int ToInt(this JobId jobId) => jobId.Value;

        private static JobId ToJobId(this int value) => new(value);
    }
}