using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    public class UpdateVillageListCommand : VillagePanelCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMediator _mediator;

        public UpdateVillageListCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dtos = Get(html);
            if (!dtos.Any()) return;

            UpdateToDatabase(accountId, dtos.ToList());
            await _mediator.Publish(new VillageUpdated(accountId), cancellationToken);
        }

        private void UpdateToDatabase(AccountId accountId, List<VillageDto> dtos)
        {
            using var context = _contextFactory.CreateDbContext();
            var villages = context.Villages
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var ids = dtos.Select(x => x.Id.Value).ToList();

            var villageDeleted = villages.Where(x => !ids.Contains(x.Id)).ToList();
            var villageInserted = dtos.Where(x => !villages.Exists(v => v.Id == x.Id.Value)).ToList();
            var villageUpdated = villages.Where(x => ids.Contains(x.Id)).ToList();

            villageDeleted.ForEach(x => context.Remove(x));
            villageInserted.ForEach(x =>
            {
                context.Add(x.ToEntity(accountId));
                context.FillVillageSettings(accountId, x.Id);
            });

            foreach (var village in villageUpdated)
            {
                var dto = dtos.Find(x => x.Id.Value == village.Id);
                dto.To(village);
                context.Update(village);
            }

            context.SaveChanges();
        }
    }
}