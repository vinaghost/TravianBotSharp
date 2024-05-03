using System.Net;

namespace MainCore.Commands.Update
{
    public class UpdateBuildingCommand : ByAccountVillageIdBase, ICommand
    {
        public UpdateBuildingCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    public class UpdateBuildingCommandHandler : ICommandHandler<UpdateBuildingCommand>
    {
        private readonly IMediator _mediator;
        private readonly IChromeManager _chromeManager;

        private readonly IQueueBuildingParser _queueBuildingParser;
        private readonly IInfrastructureParser _infrastructureParser;
        private readonly IFieldParser _fieldParser;

        private readonly IStorageRepository _storageRepository;
        private readonly IQueueBuildingRepository _queueBuildingRepository;
        private readonly IBuildingRepository _buildingRepository;

        public UpdateBuildingCommandHandler(IMediator mediator, IQueueBuildingParser queueBuildingParser, IInfrastructureParser infrastructureParser, IFieldParser fieldParser, IStorageRepository storageRepository, IQueueBuildingRepository queueBuildingRepository, IBuildingRepository buildingRepository, IChromeManager chromeManager)
        {
            _mediator = mediator;
            _chromeManager = chromeManager;
            _queueBuildingParser = queueBuildingParser;
            _infrastructureParser = infrastructureParser;
            _fieldParser = fieldParser;
            _storageRepository = storageRepository;
            _queueBuildingRepository = queueBuildingRepository;
            _buildingRepository = buildingRepository;
            _chromeManager = chromeManager;
        }

        public async Task<Result> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var villageId = request.VillageId;

            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            var dtoStorage = Get(html);

            _storageRepository.Update(villageId, dtoStorage);
            await _mediator.Publish(new StorageUpdated(accountId, villageId), cancellationToken);

            var dtoBuilding = GetBuildings(chromeBrowser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = _queueBuildingParser.Get(html);
            var queueBuildings = dtoQueueBuilding.ToList();
            var result = IsVaildQueueBuilding(queueBuildings);
            if (result.IsFailed) return result;

            _queueBuildingRepository.Update(villageId, queueBuildings);
            _buildingRepository.Update(villageId, dtoBuilding.ToList());

            var dtoUnderConstructionBuildings = dtoBuilding.Where(x => x.IsUnderConstruction).ToList();
            _queueBuildingRepository.Update(villageId, dtoUnderConstructionBuildings);
            await _mediator.Publish(new QueueBuildingUpdated(accountId, villageId), cancellationToken);

            return Result.Ok();
        }

        private IEnumerable<BuildingDto> GetBuildings(string url, HtmlDocument html)
        {
            if (url.Contains("dorf1"))
                return _fieldParser.Get(html);

            if (url.Contains("dorf2"))
                return _infrastructureParser.Get(html);

            return Enumerable.Empty<BuildingDto>();
        }

        private static Result IsVaildQueueBuilding(List<QueueBuildingDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var strType = dto.Type;
                var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
                if (!resultParse) return Stop.EnglishRequired(strType);
            }
            return Result.Ok();
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