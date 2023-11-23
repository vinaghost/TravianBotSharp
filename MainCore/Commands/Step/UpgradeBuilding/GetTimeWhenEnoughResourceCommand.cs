using FluentResults;
using MainCore.Common.Models;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;

namespace MainCore.Commands.Step.UpgradeBuilding
{
    [RegisterAsTransient]
    public class GetTimeWhenEnoughResourceCommand : IGetTimeWhenEnoughResourceCommand
    {
        private readonly IChromeManager _chromeManager;
        private readonly IUnitOfParser _unitOfParser;
        private readonly IUnitOfRepository _unitOfRepository;
        public TimeSpan Value { get; private set; }

        public GetTimeWhenEnoughResourceCommand(IChromeManager chromeManager, IUnitOfParser unitOfParser, IUnitOfRepository unitOfRepository)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfRepository = unitOfRepository;
        }

        public Result Execute(AccountId accountId, VillageId villageId, NormalBuildPlan plan)
        {
            var chromeBrowser = _chromeManager.Get(accountId);
            var html = chromeBrowser.Html;
            var isEmptySite = _unitOfRepository.BuildingRepository.EmptySite(villageId, plan.Location);
            Value = _unitOfParser.UpgradeBuildingParser.GetTimeWhenEnoughResource(html, isEmptySite, plan.Type);
            return Result.Ok();
        }
    }
}