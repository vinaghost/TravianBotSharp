using MainCore.Commands.Abstract;

namespace MainCore.Commands.Update
{
    [RegisterScoped<UpdateStorageCommand>]
    public class UpdateStorageCommand(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory, StorageUpdated.Handler storageUpdated) : CommandBase(dataService), ICommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
        private readonly StorageUpdated.Handler _storageUpdated = storageUpdated;

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var accountId = _dataService.AccountId;
            var villageId = _dataService.VillageId;
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;
            var dto = Get(html);
            UpdateToDatabase(villageId, dto);
            await _storageUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
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