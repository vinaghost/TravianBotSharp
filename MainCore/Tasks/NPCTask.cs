using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class NPCTask : VillageTask
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IVillageSettingRepository _villageSettingRepository;

        public NPCTask(IChromeManager chromeManager, IMediator mediator, IVillageRepository villageRepository, IBuildingRepository buildingRepository, IVillageSettingRepository villageSettingRepository) : base(chromeManager, mediator, villageRepository)
        {
            _buildingRepository = buildingRepository;
            _villageSettingRepository = villageSettingRepository;
        }

        protected override async Task<Result> Execute()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            Result result;

            result = await _mediator.Send(ToDorfCommand.ToDorf2(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var market = _buildingRepository.GetBuildingLocation(VillageId, BuildingEnums.Marketplace);

            result = await _mediator.Send(new ToBuildingCommand(chromeBrowser, market), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new SwitchTabCommand().Execute(chromeBrowser, 0, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await OpenNPCDialog();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await InputAmount();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await Redeem();
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new UpdateBuildingCommand(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _villageRepository.GetVillageName(VillageId);
            _name = $"NPC in {village}";
        }

        private async Task<Result> OpenNPCDialog()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = GetExchangeResourcesButton(html);
            if (button is null) return Retry.ButtonNotFound("Exchange resources");
            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            bool dialogShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                return NPCDialogShown(doc);
            }

            result = await chromeBrowser.Wait(dialogShown, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> InputAmount()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var sum = GetSum(html);
            var ratio = GetRatio();
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sum - sumValue;
            values[3] += diff;

            var inputs = GetInputs(html).ToArray();

            Result result;
            for (var i = 0; i < 4; i++)
            {
                result = await chromeBrowser.InputTextbox(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        private int[] GetRatio()
        {
            var settingNames = new List<VillageSettingEnums> {
                VillageSettingEnums.AutoNPCWood,
                VillageSettingEnums.AutoNPCClay,
                VillageSettingEnums.AutoNPCIron,
                VillageSettingEnums.AutoNPCCrop,
            };
            var settings = _villageSettingRepository.GetByName(VillageId, settingNames);

            var ratio = new int[4]
            {
                settings[VillageSettingEnums.AutoNPCWood],
                settings[VillageSettingEnums.AutoNPCClay],
                settings[VillageSettingEnums.AutoNPCIron],
                settings[VillageSettingEnums.AutoNPCCrop],
            };
            var sum = ratio.Sum();
            if (sum == 0)
            {
                ratio = Enumerable.Repeat(1, 4).ToArray();
            }

            return ratio;
        }

        public async Task<Result> Redeem()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = GetRedeemButton(html);
            if (button is null) return Retry.ButtonNotFound("redeem");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private static bool NPCDialogShown(HtmlDocument doc)
        {
            var dialog = doc.GetElementbyId("npc");
            return dialog is not null;
        }

        private static HtmlNode GetExchangeResourcesButton(HtmlDocument doc)
        {
            var npcMerchant = doc.DocumentNode.Descendants("div")
                .FirstOrDefault(x => x.HasClass("npcMerchant"));
            if (npcMerchant is null) return null;
            var button = npcMerchant.Descendants("button")
                .FirstOrDefault(x => x.HasClass("gold"));
            return button;
        }

        private static HtmlNode GetDistributeButton(HtmlDocument doc)
        {
            var submitText = doc.GetElementbyId("submitText");
            if (submitText is null) return null;
            var button = submitText.Descendants("button")
                .FirstOrDefault();
            return button;
        }

        private static HtmlNode GetRedeemButton(HtmlDocument doc)
        {
            var button = doc.GetElementbyId("npc_market_button");
            return button;
        }

        private static long GetSum(HtmlDocument doc)
        {
            var sum = doc.GetElementbyId("sum");
            if (sum is null) return -1;
            return sum.InnerText.ParseLong();
        }

        private static IEnumerable<HtmlNode> GetInputs(HtmlDocument doc)
        {
            for (int i = 0; i < 4; i++)
            {
                var node = doc.DocumentNode.Descendants("input")
                    .Where(x => x.GetAttributeValue("name", "") == $"desired{i}")
                    .FirstOrDefault();
                yield return node;
            }
        }
    }
}