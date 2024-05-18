using MainCore.Commands.Features.TrainTroop;
using MainCore.Common.Errors.TrainTroop;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTask]
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

        public TrainTroopTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            var buildings = new GetTrainTroopBuilding().Execute(VillageId);
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

            new SetSettingCommand().Execute(VillageId, settings);
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = new GetSetting().ByName(VillageId, VillageSettingEnums.TrainTroopRepeatTimeMin, VillageSettingEnums.TrainTroopRepeatTimeMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var name = new GetVillageName().Execute(VillageId);
            _name = $"Training troop in {name}";
        }

        public async Task<Result> Train(BuildingEnums buildingType)
        {
            Result result;
            result = await new ToDorfCommand().Execute(_chromeBrowser, 2, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new UpdateBuildingCommand().Execute(_chromeBrowser, AccountId, VillageId, CancellationToken);

            var buildingLocation = new GetBuildingLocation().Execute(VillageId, buildingType);
            if (buildingLocation == default)
            {
                return MissingBuilding.Error(buildingType);
            }

            result = await new ToBuildingCommand().Execute(_chromeBrowser, buildingLocation, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var troopSeting = _settings[buildingType];
            var troop = (TroopEnums)new GetSetting().ByName(VillageId, troopSeting);
            var (minSetting, maxSetting) = _amountSettings[buildingType];
            var amount = new GetSetting().ByName(VillageId, minSetting, maxSetting);

            var html = _chromeBrowser.Html;

            var maxAmount = GetMaxAmount(html, troop);

            if (maxAmount == 0)
            {
                return MissingResource.Error(buildingType);
            }

            if (amount > maxAmount)
            {
                var trainWhenLowResource = new GetSetting().BooleanByName(VillageId, VillageSettingEnums.TrainWhenLowResource);
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

            var inputBox = GetInputBox(html, troop);
            if (inputBox is null) return Retry.TextboxNotFound("troop amount input");

            result = await _chromeBrowser.InputTextbox(By.XPath(inputBox.XPath), $"{amount}");
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var trainButton = GetTrainButton(html);
            if (trainButton is null) return Retry.ButtonNotFound("train troop");

            result = await _chromeBrowser.Click(By.XPath(trainButton.XPath), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await new DelayClickCommand().Execute(AccountId);

            return Result.Ok();
        }

        private static HtmlNode GetInputBox(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return null;
            var input = cta.Descendants("input")
                .FirstOrDefault(x => x.HasClass("text"));
            return input;
        }

        private static int GetMaxAmount(HtmlDocument doc, TroopEnums troop)
        {
            var node = GetNode(doc, troop);
            var cta = node.Descendants("div")
                .FirstOrDefault(x => x.HasClass("cta"));
            if (cta is null) return 0;
            var a = cta.Descendants("a")
                .FirstOrDefault();
            if (a is null) return 0;
            return a.InnerText.ParseInt();
        }

        private static HtmlNode GetTrainButton(HtmlDocument doc)
        {
            return doc.GetElementbyId("s1");
        }

        private static HtmlNode GetNode(HtmlDocument doc, TroopEnums troop)
        {
            var nodes = doc.DocumentNode.Descendants("div")
               .Where(x => x.HasClass("troop"))
               .Where(x => !x.HasClass("empty"))
               .AsEnumerable();

            foreach (var node in nodes)
            {
                var img = node.Descendants("img")
                .FirstOrDefault(x => x.HasClass("unit"));
                if (img is null) continue;
                var classes = img.GetClasses();
                var type = classes
                    .Where(x => x.StartsWith('u'))
                    .FirstOrDefault(x => !x.Equals("unit"));
                if (type.ParseInt() == (int)troop) return node;
            }
            return null;
        }
    }
}