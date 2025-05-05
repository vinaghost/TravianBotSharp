namespace MainCore.DTO
{
    public class FarmDto
    {
        public FarmId Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class FarmMapper
    {
        public static Farm ToEntity(this FarmDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Farm.AccountId))]
        public static partial void To(this FarmDto dto, Farm entity);

        [MapperIgnoreTarget(nameof(Farm.AccountId))]
        private static partial Farm ToEntity(this FarmDto dto);

        private static int ToInt(this FarmId farmId) => farmId.Value;
    }
}