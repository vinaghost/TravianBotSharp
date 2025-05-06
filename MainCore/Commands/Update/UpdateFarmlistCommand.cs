using MainCore.Constraints;

namespace MainCore.Commands.Update
{
    [Handler]
    public static partial class UpdateFarmlistCommand
    {
        public sealed record Command(AccountId AccountId) : ICommand;

        private static async ValueTask<Result> HandleAsync(
            Command command,
            IChromeBrowser browser,
            AppDbContext context,
            FarmListUpdated.Handler farmListUpdated,
            CancellationToken cancellationToken)
        {

            var html = browser.Html;

            var dtos = Get(html);
            UpdateToDatabase(command.AccountId, dtos, context);

            await farmListUpdated.HandleAsync(new(command.AccountId), cancellationToken);
            return Result.Ok();
        }

        private static void UpdateToDatabase(AccountId accountId, IEnumerable<FarmDto> dtos, AppDbContext context)
        {

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
            var nodes = FarmListParser.GetFarmNodes(doc);
            foreach (var node in nodes)
            {
                var id = FarmListParser.GetId(node);
                var name = FarmListParser.GetName(node);
                yield return new()
                {
                    Id = id,
                    Name = name,
                };
            }
        }
    }
}