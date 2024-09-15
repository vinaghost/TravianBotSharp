using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class FarmDto
    {
        public FarmId Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    [Mapper]
    public static partial class FarmMapper
    {
        public static Farm ToEntity(this FarmDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        [MapperIgnoreSource(nameof(FarmDto.Id))]
        [MapperIgnoreSource(nameof(FarmDto.IsActive))]
        public static partial void To(this FarmDto dto, Farm entity);

        private static partial Farm ToEntity(this FarmDto dto);

        private static int ToInt(this FarmId farmId) => farmId.Value;
    }
}