using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class VillageDto
    {
        public VillageId Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsActive { get; set; }
        public bool IsUnderAttack { get; set; }
    }

    [Mapper]
    public static partial class VillageMapper
    {
        public static Village ToEntity(this VillageDto dto, AccountId accountId)
        {
            var entity = ToEntity(dto);
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial void To(this VillageDto dto, Village entity);

        public static partial VillageDto ToDto(this Village dto);

        private static partial Village ToEntity(this VillageDto dto);

        public static partial IQueryable<VillageDto> ToDto(this IQueryable<Village> entities);

        private static int ToInt(this VillageId villageId) => villageId.Value;

        private static VillageId ToVillageId(this int value) => new(value);
    }
}