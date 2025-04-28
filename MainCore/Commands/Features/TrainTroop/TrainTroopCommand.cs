using MainCore.Commands.Abstract;
using MainCore.Common.Errors.TrainTroop;

namespace MainCore.Commands.Features.TrainTroop
{
    [RegisterScoped<TrainTroopCommand>]
    public class TrainTroopCommand : CommandBase, ICommand<BuildingEnums>
    {
        private readonly ToDorfCommand _toDorfCommand;
        private readonly UpdateBuildingCommand _updateBuildingCommand;
        private readonly ToBuildingCommand _toBuildingCommand;
        private readonly IGetSetting _getSetting;
        private readonly GetBuildingLocationQuery.Handler _getBuildingLocation;

        public TrainTroopCommand(IDataService dataService, ToDorfCommand toDorfCommand, UpdateBuildingCommand updateBuildingCommand, ToBuildingCommand toBuildingCommand, IGetSetting getSetting, GetBuildingLocationQuery.Handler getBuildingLocation) : base(dataService)
        {
            _toDorfCommand = toDorfCommand;
            _updateBuildingCommand = updateBuildingCommand;
            _toBuildingCommand = toBuildingCommand;
            _getSetting = getSetting;
            _getBuildingLocation = getBuildingLocation;
        }

        public static Dictionary<BuildingEnums, VillageSettingEnums> BuildingSettings { get; } = new()
        {
            {BuildingEnums.Barracks, VillageSettingEnums.BarrackTroop },
            {BuildingEnums.Stable, VillageSettingEnums.StableTroop },
            {BuildingEnums.GreatBarracks, VillageSettingEnums.GreatBarrackTroop },
            {BuildingEnums.GreatStable, VillageSettingEnums.GreatStableTroop },
            {BuildingEnums.Workshop, VillageSettingEnums.WorkshopTroop },
        };

        private static Dictionary<BuildingEnums, (VillageSettingEnums, VillageSettingEnums)> AmountSettings { get; } = new()
        {
            {BuildingEnums.Barracks, (VillageSettingEnums.BarrackAmountMin,VillageSettingEnums.BarrackAmountMax ) },
            {BuildingEnums.Stable, (VillageSettingEnums.StableAmountMin,VillageSettingEnums.StableAmountMax ) },
            {BuildingEnums.GreatBarracks, (VillageSettingEnums.GreatBarrackAmountMin,VillageSettingEnums.GreatBarrackAmountMax ) },
            {BuildingEnums.GreatStable, (VillageSettingEnums.GreatStableAmountMin,VillageSettingEnums.GreatStableAmountMax ) },
            {BuildingEnums.Workshop, (VillageSettingEnums.WorkshopAmountMin,VillageSettingEnums.WorkshopAmountMax ) },
        };

        public async Task<Result> Execute(BuildingEnums building, CancellationToken cancellationToken)
        {
            Result result;
            result = await ToTrainBuilding(building, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSeting = BuildingSettings[building];
            var troop = (TroopEnums)_getSetting.ByName(_dataService.VillageId, troopSeting);

            var (_, isFailed, amount, errors) = GetAmount(building, troop);
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

            var buildingLocation = await _getBuildingLocation.HandleAsync(new(_dataService.VillageId, building), cancellationToken);
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
            var amount = _getSetting.ByName(villageId, minSetting, maxSetting);

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

            var trainWhenLowResource = _getSetting.BooleanByName(villageId, VillageSettingEnums.TrainWhenLowResource);
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
            result = await chromeBrowser.Input(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = TrainTroopParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}