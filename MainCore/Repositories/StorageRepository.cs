using FluentResults;
using MainCore.Common.Errors;
using MainCore.Common.Errors.Storage;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Infrasturecture.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class StorageRepository : IStorageRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public StorageRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public Result IsEnoughResource(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();
            if (storage is null) return Result.Fail(new Stop("Storage is not updated correctly"));
            var result = Result.Ok();
            if (storage.Wood < requiredResource[0]) result.WithError(new Resource("wood", storage.Wood, requiredResource[0]));
            if (storage.Clay < requiredResource[1]) result.WithError(new Resource("clay", storage.Clay, requiredResource[1]));
            if (storage.Iron < requiredResource[2]) result.WithError(new Resource("iron", storage.Iron, requiredResource[2]));
            if (storage.Crop < requiredResource[3]) result.WithError(new Resource("crop", storage.Wood, requiredResource[3]));
            if (storage.FreeCrop < requiredResource[4]) result.WithError(new FreeCrop(storage.Wood, requiredResource[4]));

            var max = requiredResource.Max();
            if (storage.Warehouse < max) result.WithError(new WarehouseLimit(storage.Warehouse, max));
            if (storage.Granary < requiredResource[3]) result.WithError(new GranaryLimit(storage.Granary, requiredResource[3]));
            return result;
        }

        public long[] GetMissingResource(VillageId villageId, long[] requiredResource)
        {
            using var context = _contextFactory.CreateDbContext();
            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();
            if (storage is null) return new long[4] { 0, 0, 0, 0 };

            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }

        public int GetGranaryPercent(VillageId villageId)
        {
            using var context = _contextFactory.CreateDbContext();
            var percent = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .Select(x => x.Crop * 1f / x.Granary)
                .FirstOrDefault();
            return (int)percent;
        }

        public void Update(VillageId villageId, StorageDto dto)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbStorage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (dbStorage is null)
            {
                var storage = dto.ToEntity(villageId);
                context.Add(storage);
            }
            else
            {
                dto.To(dbStorage);
                context.Update(dbStorage);
            }

            context.SaveChanges();
        }
    }
}