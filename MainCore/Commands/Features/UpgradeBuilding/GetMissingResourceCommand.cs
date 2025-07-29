namespace MainCore.Commands.Features.UpgradeBuilding
{
    [Handler]
    public static partial class GetMissingResourceCommand
    {
        public sealed record Command(VillageId VillageId, long[] Resource) : IVillageCommand;

        private static async ValueTask<long[]> HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (villageId, requiredResource) = command;

            var storage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (storage is null) return [0, 0, 0, 0];

            var resource = new long[4];
            if (storage.Wood < requiredResource[0]) resource[0] = requiredResource[0] - storage.Wood;
            if (storage.Clay < requiredResource[1]) resource[1] = requiredResource[1] - storage.Clay;
            if (storage.Iron < requiredResource[2]) resource[2] = requiredResource[2] - storage.Iron;
            if (storage.Crop < requiredResource[3]) resource[3] = requiredResource[3] - storage.Crop;
            return resource;
        }
    }
}