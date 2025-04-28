using MainCore.Commands.Abstract;

namespace MainCore.Commands.Features.NpcResource
{
    [RegisterScoped<NpcResourceCommand>]
    public class NpcResourceCommand : CommandBase, ICommand
    {
        private static readonly List<VillageSettingEnums> _settingNames = [
            VillageSettingEnums.AutoNPCWood,
            VillageSettingEnums.AutoNPCClay,
            VillageSettingEnums.AutoNPCIron,
            VillageSettingEnums.AutoNPCCrop,
        ];

        private readonly IGetSetting _getSetting;

        public NpcResourceCommand(IDataService dataService, IGetSetting getSetting) : base(dataService)
        {
            _getSetting = getSetting;
        }

        public async Task<Result> Execute(CancellationToken cancellationToken)
        {
            Result result;
            result = await OpenNPCDialog(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var ratio = GetRatio();

            result = await InputAmount(ratio);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await Redeem(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        private async Task<Result> OpenNPCDialog(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = NpcResourceParser.GetExchangeResourcesButton(html);
            if (button is null) return Retry.ButtonNotFound("Exchange resources");

            static bool dialogShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return NpcResourceParser.IsNpcDialog(doc);
            }

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            result = await chromeBrowser.Wait(dialogShown, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> InputAmount(long[] ratio)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var sum = NpcResourceParser.GetSum(html);
            var sumRatio = ratio.Sum();
            var values = new long[4];
            for (var i = 0; i < 4; i++)
            {
                values[i] = sum * ratio[i] / sumRatio;
            }
            var sumValue = values.Sum();
            var diff = sum - sumValue;
            values[3] += diff;

            var inputs = NpcResourceParser.GetInputs(html).ToArray();

            Result result;
            for (var i = 0; i < 4; i++)
            {
                result = await chromeBrowser.Input(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            return Result.Ok();
        }

        private long[] GetRatio()
        {
            var settings = _getSetting.ByName(_dataService.VillageId, _settingNames);

            var ratio = new long[4]
            {
                settings[VillageSettingEnums.AutoNPCWood],
                settings[VillageSettingEnums.AutoNPCClay],
                settings[VillageSettingEnums.AutoNPCIron],
                settings[VillageSettingEnums.AutoNPCCrop],
            };
            var sum = ratio.Sum();
            if (sum == 0)
            {
                ratio = Enumerable.Repeat<long>(1, 4).ToArray();
            }

            return ratio;
        }

        public async Task<Result> Redeem(CancellationToken cancellationToken)
        {
            var chromeBrowser = _dataService.ChromeBrowser;
            var html = chromeBrowser.Html;

            var button = NpcResourceParser.GetRedeemButton(html);
            if (button is null) return Retry.ButtonNotFound("redeem");

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}