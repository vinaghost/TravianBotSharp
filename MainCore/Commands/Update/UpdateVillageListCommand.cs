using MainCore.Constraints;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateVillageListCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            VillageUpdated.Handler villageUpdated,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var dtos = VillagePanelParser.Get(html);
            if (!dtos.Any()) return Result.Ok();

            UpdateToDatabase(command.AccountId, dtos.ToList(), context);

            await villageUpdated.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
        }

        private static void UpdateToDatabase(AccountId accountId, List<VillageDto> dtos, AppDbContext context)
        {
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