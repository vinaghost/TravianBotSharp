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

    [Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
    public static partial class StorageMapper
    {
        public static Storage ToEntity(this StorageDto dto, VillageId villageId)
        {
            var entity = dto.ToEntity();
            entity.VillageId = villageId.Value;
            return entity;
        }

        [MapperIgnoreTarget(nameof(Storage.Id))]
        [MapperIgnoreTarget(nameof(Storage.VillageId))]
        public static partial void To(this StorageDto dto, Storage entity);

        [MapperIgnoreTarget(nameof(Storage.Id))]
        [MapperIgnoreTarget(nameof(Storage.VillageId))]
        private static partial Storage ToEntity(this StorageDto dto);
    }
}
