using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped<UpdateFarmlistCommand>]
    public class UpdateFarmlistCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, IMediator mediator) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly IMediator _mediator = mediator;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var dtos = Get(html);
            UpdateToDatabase(accountId, dtos);
            await _mediator.Publish(new FarmListUpdated(accountId), cancellationToken);

            return Result.Ok();
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