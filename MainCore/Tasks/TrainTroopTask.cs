using MainCore.Common.Errors.TrainTroop;
using MainCore.Tasks.Base;

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
        private readonly IBuildingRepository _buildingRepository;
        private readonly IVillageSettingRepository _villageSettingRepository;
        private readonly ITroopPageParser _troopPageParser;
        private readonly DelayClickCommand _delayClickCommand;

        public TrainTroopTask(IMediator mediator, IVillageRepository villageRepository, ITaskManager taskManager, IBuildingRepository buildingRepository, IVillageSettingRepository villageSettingRepository, ITroopPageParser troopPageParser, DelayClickCommand delayClickCommand) : base(mediator, villageRepository)
        {
            _taskManager = taskManager;
            _buildingRepository = buildingRepository;
            _villageSettingRepository = villageSettingRepository;
            _troopPageParser = troopPageParser;
            _delayClickCommand = delayClickCommand;
        }

        protected override async Task<Result> Execute()
        {
            var buildings = _buildingRepository.GetTrainTroopBuilding(VillageId);
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

            _villageSettingRepository.Update(VillageId, settings);
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _villageSettingRepository.GetByName(VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var name = _villageRepository.GetVillageName(VillageId);
            _name = $"Training troop in {name}";
        }

        public async Task<Result> Train(BuildingEnums buildingType)
        {
            Result result;
            result = await new ToDorfCommand().Execute(_chromeBrowser, 2, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var buildingLocation = _buildingRepository.GetBuildingLocation(VillageId, buildingType);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(buildingType);
            }

            result = await _mediator.Send(new ToBuildingCommand(_chromeBrowser, buildingLocation), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSeting = _settings[buildingType];
            var troop = (TroopEnums)_villageSettingRepository.GetByName(VillageId, troopSeting);
            var (minSetting, maxSetting) = _amountSettings[buildingType];
            var amount = _villageSettingRepository.GetByName(VillageId, minSetting, maxSetting);

            var html = _chromeBrowser.Html;

            var maxAmount = _troopPageParser.GetMaxAmount(html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(buildingType);
            }

            if (amount > maxAmount)
            {
                var trainWhenLowResource = _villageSettingRepository.GetBooleanByName(VillageId, VillageSettingEnums.TrainWhenLowResource);
                if (trainWhenLowResource)
                {
                    amount = maxAmount;
                }
                else
                {
                    return MissingResource.Error(buildingType);
                }
            }

            html = _chromeBrowser.Html;

            var inputBox = _troopPageParser.GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            result = await _chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = _troopPageParser.GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await _chromeBrowser.Click(By.XPath(trainButton.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _delayClickCommand.Execute(AccountId);

            return Result.Ok();
        }
    }
}