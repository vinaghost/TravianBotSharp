using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.StartFarmList
{
    public class ToFarmListPageCommand : FarmListCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMediator _mediator;

        public ToFarmListPageCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToPage(chromeBrowser, accountId, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await Update(chromeBrowser, accountId, cancellationToken);
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

        private async Task Update(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = Get(html);
            UpdateToDatabase(accountId, dtos);
            await _mediator.Publish(new FarmListUpdated(accountId), cancellationToken);
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

        private void UpdateToDatabase(AccountId accountId, IEnumerable<FarmDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var farms = context.FarmLists
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var farmDeleted = farms.Where(x => !ids.Contains(x.Id)).ToList();
            var farmInserted = dtos.Where(x => !farms.Exists(v => v.Id == x.Id.Value)).ToList();
            var farmUpdated = farms.Where(x => ids.Contains(x.Id)).ToList();

            farmDeleted.ForEach(x => context.Remove(x));
            farmInserted.ForEach(x => context.Add(x.ToEntity(accountId)));

            foreach (var farm in farmUpdated)
            {
                var dto = dtos.FirstOrDefault(x => x.Id.Value == farm.Id);
                dto.To(farm);
                context.Update(farm);
            }

            context.SaveChanges();
        }

        private static IEnumerable<FarmDto> Get(HtmlDocument doc)
        {
            var nodes = GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = GetId(node);
                var name = GetName(node);
                yield return new()
                {
                    Id = id,
                    Name = name,
                };
            }
        }

        private static string GetName(HtmlNode node)
        {
            var flName = node
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("name"));
            if (flName is null) return null;
            return flName.InnerText.Trim();
        }
    }
}