using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    public class UpdateFarmlistCommand : FarmListCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMediator _mediator;

        public UpdateFarmlistCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            await Update(chromeBrowser, accountId, cancellationToken);
        }

        private async Task Update(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = Get(html);
            UpdateToDatabase(accountId, dtos);
            await _mediator.Publish(new FarmListUpdated(accountId), cancellationToken);
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
    }
}