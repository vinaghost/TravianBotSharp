using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class UpdateStorageCommand(DataService dataService, IDbContextFactory<AppDbContext> contextFactory, IMediator mediator) : CommandBase(dataService)
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly IMediator _mediator = mediator;

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var dto = Get(html);
            UpdateToDatabase(villageId, dto);
            await _mediator.Publish(new StorageUpdated(accountId, villageId), cancellationToken);
            return Result.Ok();
        }

        private static StorageDto Get(HtmlDocument doc)
        {
            var storage = new StorageDto()
            {
                Wood = StorageParser.GetWood(doc),
                Clay = StorageParser.GetClay(doc),
                Iron = StorageParser.GetIron(doc),
                Crop = StorageParser.GetCrop(doc),
                FreeCrop = StorageParser.GetFreeCrop(doc),
                Warehouse = StorageParser.GetWarehouseCapacity(doc),
                Granary = StorageParser.GetGranaryCapacity(doc)
            };
            return storage;
        }

        private void UpdateToDatabase(VillageId villageId, StorageDto dto)
        {
            using var context = _contextFactory.CreateDbContext();
            var dbStorage = context.Storages
                .Where(x => x.VillageId == villageId.Value)
                .FirstOrDefault();

            if (dbStorage is null)
            {
                var storage = dto.ToEntity(villageId);
                context.Add(storage);
            }
            else
            {
                dto.To(dbStorage);
                context.Update(dbStorage);
            }

            context.SaveChanges();
        }
    }
}