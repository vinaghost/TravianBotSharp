using System.Text.RegularExpressions;

namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateInventoryCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask HandleAsync(Command command)
        {
            var dtos = await InventoryParser.GetItems(browser.CurrentPage);
            Update(command.AccountId, dtos);
        }

        private void Update(AccountId accountId, List<HeroItemDto> dtos)
        {
            using var context = contextFactory.CreateDbContext();
            var items = context.HeroItems
                .Where(x => x.AccountId == accountId.Value)
                .ToList();

            var types = dtos.Select(x => x.Type).ToList();

            var itemDeleted = items.Where(x => !types.Contains(x.Type)).ToList();
            var itemInserted = dtos.Where(x => !items.Exists(v => v.Type == x.Type)).ToList();
            var itemUpdated = items.Where(x => types.Contains(x.Type)).ToList();

            itemDeleted.ForEach(x => context.Remove(x));
            itemInserted.ForEach(x => context.Add(x.ToEntity(accountId)));

            foreach (var item in itemUpdated)
            {
                var dto = dtos.First(x => x.Type == item.Type);
                dto.To(item);
            }

            context.SaveChanges();
        }
    }
}