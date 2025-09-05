namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class ValidateEnoughResourceCommand
    {
        public sealed record Command(VillageId VillageId, long[] Resource) : IVillageCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, resource) = command;

            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (storage is null) return Result.Ok();

            var errors = new List<Error>();
            if (storage.Wood < resource[0])
            {
                errors.Add(MissingResource.Wood(storage.Wood, resource[0]));
            }

            if (storage.Clay < resource[1])
            {
                errors.Add(MissingResource.Clay(storage.Clay, resource[1]));
            }

            if (storage.Iron < resource[2])
            {
                errors.Add(MissingResource.Iron(storage.Iron, resource[2]));
            }

            if (storage.Crop < resource[3])
            {
                errors.Add(MissingResource.Crop(storage.Crop, resource[3]));
            }

            if (resource.Length == 5 && storage.FreeCrop < resource[4])
            {
                errors.Add(LackOfFreeCrop.Error(storage.FreeCrop, resource[4]));
            }

            if (storage.Granary < resource[3])
            {
                errors.Add(StorageLimit.Granary(storage.Granary, resource[3]));
            }

            var max = resource.Take(3).Max();
            if (storage.Warehouse < max)
            {
                errors.Add(StorageLimit.Warehouse(storage.Warehouse, max));
            }

            if (errors.Count > 0) return Result.Fail(errors);

            return Result.Ok();
        }
    }
}
