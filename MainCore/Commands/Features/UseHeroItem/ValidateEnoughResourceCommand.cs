namespace MainCore.Commands.Features.UseHeroItem
{
    [Handler]
    public static partial class ValidateEnoughResourceCommand
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
            AppDbContext context,
            CancellationToken cancellationToken
        )
        {
            await Task.CompletedTask;
            var (accountId, resource) = command;

            var resourceItems = context.HeroItems
               .Where(x => x.AccountId == accountId.Value)
               .Where(x => ResourceItemTypes.Contains(x.Type))
               .OrderBy(x => x.Type)
               .ToList();

            var errors = new List<Error>();
            for (var i = 0; i < 4; i++)
            {
                var type = ResourceItemTypes[i];
                var item = resourceItems.Find(x => x.Type == type);
                var amount = item?.Amount ?? 0;
                if (amount < resource[i])
                {
                    errors.Add(MissingResource.Error($"{type}", amount, resource[i]));
                }
            }

            if (errors.Count > 0) return Result.Fail(errors);

            return Result.Ok();
        }
    }
}