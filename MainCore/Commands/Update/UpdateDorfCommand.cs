using FluentResults;
using HtmlAgilityPack;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.DTO;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Notification.Message;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MediatR;

namespace MainCore.Commands.Update
{
    [RegisterAsTransient]
    public class UpdateDorfCommand : UpdateCommandBase, IUpdateDorfCommand
    {
        public UpdateDorfCommand(IChromeManager chromeManager, IMediator mediator, IUnitOfRepository unitOfRepository, IUnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Execute(AccountId accountId, VillageId villageId)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;

            var dtoBuilding = GetBuildings(chromeBrowser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = _unitOfParser.QueueBuildingParser.Get(html);
            var dtoStorage = _unitOfParser.StockBarParser.Get(html);

            var queueBuildings = dtoQueueBuilding.ToList();
            var result = IsVaild(queueBuildings);
            if (result.IsFailed) return result;

            _unitOfRepository.QueueBuildingRepository.Update(villageId, queueBuildings);

            _unitOfRepository.BuildingRepository.Update(villageId, dtoBuilding.ToList());

            var dtoUnderConstructionBuildings = dtoBuilding.Where(x => x.IsUnderConstruction).ToList();
            _unitOfRepository.QueueBuildingRepository.Update(villageId, dtoUnderConstructionBuildings);
            await _mediator.Publish(new QueueBuildingUpdated(accountId, villageId));

            _unitOfRepository.StorageRepository.Update(villageId, dtoStorage);
            await _mediator.Publish(new StorageUpdated(accountId, villageId));

            return Result.Ok();
        }

        private IEnumerable<BuildingDto> GetBuildings(string url, HtmlDocument html)
        {
            if (url.Contains("dorf1"))
                return _unitOfParser.FieldParser.Get(html);

            if (url.Contains("dorf2"))
                return _unitOfParser.InfrastructureParser.Get(html);

            return Enumerable.Empty<BuildingDto>();
        }

        private static Result IsVaild(List<QueueBuildingDto> dtos)
        {
            foreach (var dto in dtos)
            {
                var strType = dto.Type;
                var resultParse = Enum.TryParse(strType, false, out BuildingEnums _);
                if (!resultParse) return Result.Fail(Stop.EnglishRequired(strType));
            }
            return Result.Ok();
        }
    }
}