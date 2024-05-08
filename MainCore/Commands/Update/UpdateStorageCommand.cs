using System.Net;

namespace MainCore.Commands.Update
{
    public class UpdateStorageCommand
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly IMediator _mediator;

        public UpdateStorageCommand(IDbContextFactory<AppDbContext> contextFactory = null, IMediator mediator = null)
        {
            _contextFactory = contextFactory ?? Locator.Current.GetService<IDbContextFactory<AppDbContext>>();
            _mediator = mediator ?? Locator.Current.GetService<IMediator>();
        }

        public async Task Execute(IChromeBrowser chromeBrowser, AccountId accountId, VillageId villageId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var dto = Get(html);
            UpdateToDatabase(villageId, dto);
            await _mediator.Publish(new StorageUpdated(accountId, villageId), cancellationToken);
        }

        private static StorageDto Get(HtmlDocument doc)
        {
            var storage = new StorageDto()
            {
                Wood = GetWood(doc),
                Clay = GetClay(doc),
                Iron = GetIron(doc),
                Crop = GetCrop(doc),
                FreeCrop = GetFreeCrop(doc),
                Warehouse = GetWarehouseCapacity(doc),
                Granary = GetGranaryCapacity(doc)
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

        private static long GetResource(HtmlDocument doc, string id)
        {
            var node = doc.GetElementbyId(id);
            if (node is null) return -1;
            return node.InnerText.ParseLong();
        }

        private static long GetWood(HtmlDocument doc) => GetResource(doc, "l1");

        private static long GetClay(HtmlDocument doc) => GetResource(doc, "l2");

        private static long GetIron(HtmlDocument doc) => GetResource(doc, "l3");

        private static long GetCrop(HtmlDocument doc) => GetResource(doc, "l4");

        private static long GetFreeCrop(HtmlDocument doc) => GetResource(doc, "stockBarFreeCrop");

        private static long GetWarehouseCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var warehouseNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("warehouse"));
            if (warehouseNode is null) return -1;
            var capacityNode = warehouseNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            return valueNode.InnerText.ParseLong();
        }

        private static long GetGranaryCapacity(HtmlDocument doc)
        {
            var stockBarNode = doc.GetElementbyId("stockBar");
            if (stockBarNode is null) return -1;
            var granaryNode = stockBarNode.Descendants("div").FirstOrDefault(x => x.HasClass("granary"));
            if (granaryNode is null) return -1;
            var capacityNode = granaryNode.Descendants("div").FirstOrDefault(x => x.HasClass("capacity"));
            if (capacityNode is null) return -1;
            var valueNode = capacityNode.Descendants("div").FirstOrDefault(x => x.HasClass("value"));
            if (valueNode is null) return -1;
            var valueStrFixed = WebUtility.HtmlDecode(valueNode.InnerText);
            if (string.IsNullOrEmpty(valueStrFixed)) return -1;
            return valueNode.InnerText.ParseLong();
        }
    }
}