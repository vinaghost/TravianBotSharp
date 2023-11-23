using MainCore.Entities;
using Riok.Mapperly.Abstractions;

namespace MainCore.DTO
{
    public class StorageDto
    {
        public long Wood { get; set; }
        public long Clay { get; set; }
        public long Iron { get; set; }
        public long Crop { get; set; }
        public long Warehouse { get; set; }
        public long Granary { get; set; }
        public long FreeCrop { get; set; }
    }

    [Mapper]
    public static partial class StorageMapper
    {
        public static Storage ToEntity(this StorageDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        public static partial void To(this StorageDto dto, Storage entity);

        private static partial Storage ToEntity(this StorageDto dto);
    }
}