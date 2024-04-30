using HtmlAgilityPack;
using MainCore.Commands.Base;

namespace MainCore.Commands.Features.Step.UpgradeBuilding.SpecialUpgradeCommandHandler
{
    [RegisterAsTransient(ServerEnums.TravianOfficial)]
    public class TravianOfficial : ICommandHandler<SpecialUpgradeCommand>
    {
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;
        private readonly UnitOfCommand _unitOfCommand;

        public TravianOfficial(IChromeManager chromeManager, UnitOfParser unitOfParser, UnitOfCommand unitOfCommand)
        {
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _unitOfCommand = unitOfCommand;
        }

        public async Task<Result> Handle(SpecialUpgradeCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            var html = chromeBrowser.Html;
            var button = _unitOfParser.UpgradeBuildingParser.GetSpecialUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("Watch ads upgrade");
            var result = await chromeBrowser.Click(By.XPath(button.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var driver = chromeBrowser.Driver;
            var current = driver.CurrentWindowHandle;
            while (driver.WindowHandles.Count > 1)
            {
                var others = driver.WindowHandles.FirstOrDefault(x => !x.Equals(current));
                driver.SwitchTo().Window(others);
                driver.Close();
                driver.SwitchTo().Window(current);
            }

            bool videoFeatureShown(IWebDriver driver)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(driver.PageSource);
                return doc.GetElementbyId("videoFeature") is not null;
            };

            result = await chromeBrowser.Wait(videoFeatureShown, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var videoFeature = html.GetElementbyId("videoFeature");
            if (videoFeature.HasClass("infoScreen"))
            {
                var checkbox = videoFeature.Descendants("div").FirstOrDefault(x => x.HasClass("checkbox"));
                if (checkbox is null) return Retry.ButtonNotFound("Don't show watch ads confirm again");
                result = await chromeBrowser.Click(By.XPath(checkbox.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var watchButton = videoFeature.Descendants("button").FirstOrDefault(x => x.HasClass("green"));
                if (watchButton is null) return Retry.ButtonNotFound("Watch ads");
                result = await chromeBrowser.Click(By.XPath(watchButton.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            await Task.Delay(Random.Shared.Next(20_000, 25_000), CancellationToken.None);

            html = chromeBrowser.Html;
            var node = html.GetElementbyId("videoFeature");
            if (node is null) return Retry.ButtonNotFound($"play ads");

            result = await chromeBrowser.Click(By.XPath(node.XPath));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            driver.SwitchTo().DefaultContent();

            // close if bot click on playing ads
            // chrome will open new tab & pause ads
            do
            {
                var handles = driver.WindowHandles;
                if (handles.Count == 1) break;

                current = driver.CurrentWindowHandle;
                var other = driver.WindowHandles.FirstOrDefault(x => !x.Equals(current));
                driver.SwitchTo().Window(other);
                driver.Close();
                driver.SwitchTo().Window(current);

                result = await chromeBrowser.Click(By.XPath(node.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                driver.SwitchTo().DefaultContent();
            }
            while (true);

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var dontShowThisAgain = html.GetElementbyId("dontShowThisAgain");
            if (dontShowThisAgain is not null)
            {
                result = await chromeBrowser.Click(By.XPath(dontShowThisAgain.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await _unitOfCommand.DelayClickCommand.Handle(new(command.AccountId), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var okButton = html.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("dialogButtonOk"));
                if (okButton is null) return Retry.ButtonNotFound("ok");
                result = await chromeBrowser.Click(By.XPath(okButton.XPath));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }
    }
}