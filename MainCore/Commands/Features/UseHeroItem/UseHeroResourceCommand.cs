namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class UseHeroResourceCommand
    {
        public sealed record Command(AccountId AccountId, long[] Resource) : IAccountCommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            ToHeroInventoryCommand.Handler toHeroInventoryCommand,
            UpdateInventoryCommand.Handler updateInventoryCommand,
            ValidateEnoughResourceCommand.Handler validateEnoughResourceCommand,
            UseHeroItemCommand.Handler useHeroItemCommand,
            IDelayService delayService,
            CancellationToken cancellationToken)
        {
            var (accountId, resource) = command;

            var result = await toHeroInventoryCommand.HandleAsync(new(), cancellationToken);
            if (result.IsFailed) return result;

            await updateInventoryCommand.HandleAsync(new(accountId), cancellationToken);

            resource = resource.Select(RoundUpTo100).ToArray();

            result = await validateEnoughResourceCommand.HandleAsync(new(accountId, resource), cancellationToken);
            if (result.IsFailed) return result;

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
                result = await useHeroItemCommand.HandleAsync(new(item, amount), cancellationToken);
                if (result.IsFailed) return result;
            }

            await delayService.DelayClick(cancellationToken);
            return Result.Ok();
        }

        private static long RoundUpTo100(long res)
        {
            if (res == 0) return 0;
            var remainder = res % 100;
            return res + (100 - remainder);
        }
    }
}