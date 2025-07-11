namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroResourceCommand
    {
        public sealed record Command(AccountId AccountId, long[] Resource) : IAccountCommand;

        private static readonly List<HeroItemEnums> ResourceItemTypes = new()
        {
            HeroItemEnums.Wood,
            HeroItemEnums.Clay,
            HeroItemEnums.Iron,
            HeroItemEnums.Crop,
        };

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToHeroInventoryCommand.Handler toHeroInventoryCommand,
            UpdateInventoryCommand.Handler updateInventoryCommand,
            UseHeroItemCommand.Handler useHeroItemCommand,
            CancellationToken cancellationToken)
        {
            var (accountId, resource) = command;

            var result = await toHeroInventoryCommand.HandleAsync(new(), cancellationToken);
            if (result.IsFailed) return result;

            var items = await updateInventoryCommand.HandleAsync(new(accountId), cancellationToken);
            var resourceItems = items
             .Where(x => ResourceItemTypes.Contains(x.Type))
             .OrderBy(x => x.Type)
             .ToList();

            result = IsEnoughResource(resourceItems, resource);
            if (result.IsFailed) return result;

            resource = resource.Select(RoundUpTo100).ToArray();

            var itemsToUse = new Dictionary<HeroItemEnums, long>
            {
                { HeroItemEnums.Wood, resource[0] },
                { HeroItemEnums.Clay, resource[1] },
                { HeroItemEnums.Iron, resource[2] },
                { HeroItemEnums.Crop, resource[3] },
            };

            foreach (var (item, amount) in itemsToUse)
            {
                if (amount == 0) continue;
                await useHeroItemCommand.HandleAsync(new(item, amount), cancellationToken);
            }

            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }

        private static Result IsEnoughResource(
           List<HeroItemDto> items,
           long[] requiredResource)
        {
            var errors = new List<Error>();
            for (var i = 0; i < 4; i++)
            {
                var type = ResourceItemTypes[i];
                var item = items.Find(x => x.Type == type);
                var amount = item?.Amount ?? 0;
                if (amount < requiredResource[i])
                {
                    errors.Add(MissingResource.Error($"{type}", amount, requiredResource[i]));
                }
            }

            if (errors.Count > 0) return Result.Fail(errors);

            return Result.Ok();
        }
    }
}