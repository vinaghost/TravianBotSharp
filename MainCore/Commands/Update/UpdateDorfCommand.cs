using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands.Base;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.MediatR;
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
    public class UpdateDorfCommand : ByAccountVillageIdBase, ICommand
    {
        public UpdateDorfCommand(AccountId accountId, VillageId villageId) : base(accountId, villageId)
        {
        }
    }

    [RegisterAsTransient]
    public class UpdateDorfCommandHandler : UpdateCommandHandlerBase, ICommandHandler<UpdateDorfCommand>
    {
        public UpdateDorfCommandHandler(IChromeManager chromeManager, IMediator mediator, IUnitOfRepository unitOfRepository, IUnitOfParser unitOfParser) : base(chromeManager, mediator, unitOfRepository, unitOfParser)
        {
        }

        public async Task<Result> Handle(UpdateDorfCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;

            var dtoBuilding = GetBuildings(chromeBrowser.CurrentUrl, html);
            if (!dtoBuilding.Any()) return Result.Ok();

            var dtoQueueBuilding = _unitOfParser.QueueBuildingParser.Get(html);
            var dtoStorage = _unitOfParser.StockBarParser.Get(html);

            var queueBuildings = dtoQueueBuilding.ToList();
            var result = IsVaild(queueBuildings);
            if (result.IsFailed) return result;

            _unitOfRepository.QueueBuildingRepository.Update(command.VillageId, queueBuildings);

            _unitOfRepository.BuildingRepository.Update(command.VillageId, dtoBuilding.ToList());

            var dtoUnderConstructionBuildings = dtoBuilding.Where(x => x.IsUnderConstruction).ToList();
            _unitOfRepository.QueueBuildingRepository.Update(command.VillageId, dtoUnderConstructionBuildings);
            await _mediator.Publish(new QueueBuildingUpdated(command.AccountId, command.VillageId), cancellationToken);

            _unitOfRepository.StorageRepository.Update(command.VillageId, dtoStorage);
            await _mediator.Publish(new StorageUpdated(command.AccountId, command.VillageId), cancellationToken);

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