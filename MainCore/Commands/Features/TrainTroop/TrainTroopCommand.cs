﻿using MainCore.Commands.Abstract;
using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [RegisterScoped(Registration = RegistrationStrategy.Self)]
    public class TrainTroopCommand(DataService dataService, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, ToBuildingCommand toBuildingCommand) : CommandBase<BuildingEnums>(dataService)
    {
        private readonly ToDorfCommand _toDorfCommand = toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand = updateBuildingCommand;
        private readonly ToBuildingCommand _toBuildingCommand = toBuildingCommand;

        public static Dictionary<BuildingEnums, VillageSettingEnums> BuildingSettings { get; } = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> AmountSettings { get; } = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax )},
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        public override async Task<Result> Execute(CancellationToken cancellationToken)
        {
            var buildingType = Data;

            Result result;
            result = await ToTrainBuilding(buildingType, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSeting = BuildingSettings[buildingType];
            var troop = (TroopEnums)new GetSetting().ByName(_dataService.VillageId, troopSeting);

            var (_, isFailed, amount, errors) = GetAmount(buildingType, troop);
            if (isFailed) return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await TrainTroop(troop, amount, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private async Task<Result> ToTrainBuilding(BuildingEnums building, CancellationToken cancellationToken)
        {
            Result result;
            result = await _toDorfCommand.Execute(2, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var buildingLocation = new GetBuildingLocation().Execute(_dataService.VillageId, building);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(building);
            }

            result = await _toBuildingCommand.Execute(buildingLocation, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private Result<long> GetAmount(BuildingEnums building, TroopEnums troop)
        {
            var villageId = _dataService.VillageId;
            var (minSetting, maxSetting) = AmountSettings[building];
            var amount = new GetSetting().ByName(villageId, minSetting, maxSetting);

            var html = _dataService.ChromeBrowser.Html;

            var maxAmount = TrainTroopParser.GetMaxAmount(html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(building);
            }

            if (amount < maxAmount)
            {
                return amount;
            }

            var trainWhenLowResource = new GetSetting().BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
            if (!trainWhenLowResource)
            {
                return MissingResource.Error(building);
            }

            amount = maxAmount;
            return amount;
        }

        private async Task<Result> TrainTroop(TroopEnums troop, long amount, CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var inputBox = TrainTroopParser.GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            Result result;
            result = await chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = TrainTroopParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await chromeBrowser.Click(By.XPath(trainButton.XPath), cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}