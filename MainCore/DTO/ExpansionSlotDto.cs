using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class ExpansionSlotDto
    {
        public int Id { get; set; }
        public ExpansionStatusEnum Status { get; set; }
        public string Content { get; set; }
    }

    [Mapper]
    public static partial class ExpansionSlotMapper
    {
        public static ExpansionSlot ToEntity(this ExpansionSlotDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        public static partial void To(this ExpansionSlotDto dto, ExpansionSlot entity);

        private static partial ExpansionSlot ToEntity(this ExpansionSlotDto dto);

        public static partial IQueryable<ExpansionSlotDto> ToDto(this IQueryable<ExpansionSlot> entities);
    }
}