using MainCore.Errors.Storage;

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

    public static class StorageDtoExtensions
    {
        public static Result IsResourceEnough(this StorageDto storage, long[] requiredResource)
        {
            if (storage is null) return Result.Ok();

            var result = Result.Ok();
            if (storage.Wood < requiredResource[0])
            {
                result.WithError(Resource.Error("wood", storage.Wood, requiredResource[0]));
            }

            if (storage.Clay < requiredResource[1])
            {
                result.WithError(Resource.Error("clay", storage.Clay, requiredResource[1]));
            }

            if (storage.Iron < requiredResource[2])
            {
                result.WithError(Resource.Error("iron", storage.Iron, requiredResource[2]));
            }

            if (storage.Crop < requiredResource[3])
            {
                result.WithError(Resource.Error("crop", storage.Wood, requiredResource[3]));
            }

            if (requiredResource.Length == 5 && storage.FreeCrop < requiredResource[4])
            {
                result.WithError(FreeCrop.Error(storage.FreeCrop, requiredResource[4]));
            }

            if (storage.Granary < requiredResource[3])
            {
                result.WithError(StorageLimit.Error("granary", storage.Granary, requiredResource[3]));
            }

            var max = requiredResource.Take(3).Max();
            if (storage.Warehouse < max)
            {
                result.WithError(StorageLimit.Error("warehouse", storage.Warehouse, max));
            }

            return result;
        }

        public static long[] GetMissingResource(this StorageDto storage, long[] requiredResource)
        {
            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }
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