using HtmlAgilityPack;
using MainCore.DTO;

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
        private readonly IStockBarParser _stockBarParser;
        private readonly IInfrastructureParser _infrastructureParser;
        private readonly IFieldParser _fieldParser;

        private readonly IStorageRepository _storageRepository;
        private readonly IQueueBuildingRepository _queueBuildingRepository;
        private readonly IBuildingRepository _buildingRepository;

        public UpdateBuildingCommandHandler(IMediator mediator, IQueueBuildingParser queueBuildingParser, IStockBarParser stockBarParser, IInfrastructureParser infrastructureParser, IFieldParser fieldParser, IStorageRepository storageRepository, IQueueBuildingRepository queueBuildingRepository, IBuildingRepository buildingRepository, IChromeManager chromeManager)
        {
            _mediator = mediator;
            _chromeManager = chromeManager;
            _queueBuildingParser = queueBuildingParser;
            _stockBarParser = stockBarParser;
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
            var dtoStorage = _stockBarParser.Get(html);

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
    }
}