using MainCore.Common.Enums;
using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class AdventureDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public DifficultyEnums Difficulty { get; set; }
    }

    [Mapper]
    public static partial class AdventureMapper
    {
        public static Adventure ToEntity(this AdventureDto dto, AccountId accountId)
        {
            var entity = dto.ToEntity();
            entity.AccountId = accountId.Value;
            return entity;
        }

        public static partial void To(this AdventureDto dto, Adventure entity);

        private static partial Adventure ToEntity(this AdventureDto dto);

        public static partial IQueryable<AdventureDto> ToDto(this IQueryable<Adventure> entities);
    }
}