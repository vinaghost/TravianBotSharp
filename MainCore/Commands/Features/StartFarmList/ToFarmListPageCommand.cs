using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    public class ToFarmListPageCommand : FarmListCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public ToFarmListPageCommand(IDbContextFactory<AppDbContext> contextFactory = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
        }

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToPage(chromeBrowser, accountId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ToPage(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            Result result;

            var rallypointVillageId = GetVillageHasRallypoint(accountId);
            if (rallypointVillageId == VillageId.Empty) return Skip.NoRallypoint;

            result = await new SwitchVillageCommand().Execute(chromeBrowser, rallypointVillageId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToDorfCommand().Execute(chromeBrowser, 2, false, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new UpdateBuildingCommand().Execute(chromeBrowser, accountId, rallypointVillageId, cancellationToken);

            result = await new ToBuildingCommand().Execute(chromeBrowser, 39, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new SwitchTabCommand().Execute(chromeBrowser, 4, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new DelayClickCommand().Execute(accountId);
            return Result.Ok();
        }

        private VillageId GetVillageHasRallypoint(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();

            var village = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .Include(x => x.Buildings.Where(x => x.Type == BuildingEnums.RallyPoint && x.Level > 0))
                .Where(x => x.Buildings.Count > 0)
                .OrderByDescending(x => x.IsActive)
                .Select(x => x.Id)
                .AsEnumerable()
                .Select(x => new VillageId(x))
                .FirstOrDefault();
            return village;
        }
    }
}