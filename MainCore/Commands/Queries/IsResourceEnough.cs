using MainCore.Commands.Abstract;
using MainCore.Common.Errors.Storage;

namespace MainCore.Commands.Queries
{
    public class IsResourceEnough(IDbContextFactory<AppDbContext> contextFactory) : QueryBase(contextFactory)
    {
        public Result Execute(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

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
    }
}