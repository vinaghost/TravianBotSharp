using FluentResults;
using HtmlAgilityPack;
using MainCore.Commands;
using MainCore.Common.Enums;
using MainCore.Common.Errors;
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
    public class NPCTask : VillageTask
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public NPCTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, IChromeManager chromeManager, UnitOfParser unitOfParser) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 2), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var market = _unitOfRepository.BuildingRepository.GetBuildingLocation(VillageId, BuildingEnums.Marketplace);

            result = await _unitOfCommand.ToBuildingCommand.Handle(new(AccountId, market), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.SwitchTabCommand.Handle(new(AccountId, 0), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await OpenNPCDialog();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await InputAmount();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await Redeem();
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateVillageInfoCommand.Handle(new(AccountId, VillageId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"NPC in {village}";
        }

        private async Task<Result> OpenNPCDialog()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var button = _unitOfParser.MarketParser.GetExchangeResourcesButton(html);
            if (button is null) return Result.Fail(Retry.ButtonNotFound("Exchange resources"));
            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            bool dialogShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);

                return _unitOfParser.MarketParser.NPCDialogShown(doc);
            }

            result = await chromeBrowser.Wait(dialogShown, CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        public async Task<Result> InputAmount()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var sum = _unitOfParser.MarketParser.GetSum(html);
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

            var inputs = _unitOfParser.MarketParser.GetInputs(html).ToArray();

            Result result;
            for (var i = 0; i < 4; i++)
            {
                result = await chromeBrowser.InputTextbox(By.XPath(inputs[i].XPath), $"{values[i]}");
                if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
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
            var settings = _unitOfRepository.VillageSettingRepository.GetByName(VillageId, settingNames);

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

            var button = _unitOfParser.MarketParser.GetRedeemButton(html);
            if (button is null) return Result.Fail(Retry.ButtonNotFound("redeem"));

            Result result;
            result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }
    }
}