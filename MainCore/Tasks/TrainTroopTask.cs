using FluentResults;
using MainCore.Commands;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;
using OpenQA.Selenium;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class TrainTroopTask : VillageTask
    {
        private static readonly Dictionary<BuildingEnums, VillageSettingEnums> _settings = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static readonly Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> _amountSettings = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax )},
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public TrainTroopTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            var buildings = _unitOfRepository.BuildingRepository.GetTrainTroopBuilding(VillageId);
            if (buildings.Count == 0) return Result.Ok();

            Result result;
            var settings = new Dictionary<VillageSettingEnums, int>();
            foreach (var building in buildings)
            {
                result = await Train(building);
                if (result.IsFailed)
                {
                    if (result.HasError<MissingBuilding>())
                    {
                        settings.Add(_settings[building], 0);
                    }
                    else if (result.HasError<MissingResource>())
                    {
                        break;
                    }
                }
            }

            _unitOfRepository.VillageSettingRepository.Update(VillageId, settings);
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _unitOfRepository.VillageSettingRepository.GetByName(VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var name = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Training troop in {name}";
        }

        public async Task<Result> Train(BuildingEnums buildingType)
        {
            Result result;
            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var buildingLocation = _unitOfRepository.BuildingRepository.GetBuildingLocation(VillageId, buildingType);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(buildingType);
            }
            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, buildingLocation), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSeting = _settings[buildingType];
            var troop = (TroopEnums)_unitOfRepository.VillageSettingRepository.GetByName(VillageId, troopSeting);
            var (minSetting, maxSetting) = _amountSettings[buildingType];
            var amount = _unitOfRepository.VillageSettingRepository.GetByName(VillageId, minSetting, maxSetting);

            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var maxAmount = _unitOfParser.TroopPageParser.GetMaxAmount(html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(buildingType);
            }

            if (amount > maxAmount)
            {
                var trainWhenLowResource = _unitOfRepository.VillageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.TrainWhenLowResource);
                if (trainWhenLowResource)
                {
                    amount = maxAmount;
                }
                else
                {
                    return MissingResource.Error(buildingType);
                }
            }

            html = chromeBrowser.Html;

            var inputBox = _unitOfParser.TroopPageParser.GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            result = await chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = _unitOfParser.TroopPageParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _unitOfCommand.DelayClickCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}