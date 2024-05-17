namespace MainCore.Commands.Features.UpgradeBuilding
{
    public class SpecialUpgradeCommand
    {
        public async Task<Result> Execute(IChromeBrowser chromeBrowser, AccountId accountId, CancellationToken cancellationToken)
        {
            var html = chromeBrowser.Html;
            var button = GetSpecialUpgradeButton(html);
            if (button is null) return Retry.ButtonNotFound("Watch ads upgrade");
            var result = await chromeBrowser.Click(By.XPath(button.XPath), cancellationToken);
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
            }

            result = await chromeBrowser.Wait(videoFeatureShown, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var videoFeature = html.GetElementbyId("videoFeature");
            if (videoFeature.HasClass("infoScreen"))
            {
                var checkbox = videoFeature.Descendants("div").FirstOrDefault(x => x.HasClass("checkbox"));
                if (checkbox is null) return Retry.ButtonNotFound("Don't show watch ads confirm again");
                result = await chromeBrowser.Click(By.XPath(checkbox.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new DelayClickCommand().Execute(accountId);

                var watchButton = videoFeature.Descendants("button").FirstOrDefault(x => x.HasClass("green"));
                if (watchButton is null) return Retry.ButtonNotFound("Watch ads");
                result = await chromeBrowser.Click(By.XPath(watchButton.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            await Task.Delay(Random.Shared.Next(20_000, 25_000), CancellationToken.None);

            html = chromeBrowser.Html;
            var node = html.GetElementbyId("videoFeature");
            if (node is null) return Retry.ButtonNotFound($"play ads");

            result = await chromeBrowser.Click(By.XPath(node.XPath), cancellationToken);
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

                result = await chromeBrowser.Click(By.XPath(node.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                driver.SwitchTo().DefaultContent();
            }
            while (true);

            result = await chromeBrowser.WaitPageChanged("dorf", cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await chromeBrowser.WaitPageLoaded(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            await Task.Delay(Random.Shared.Next(5_000, 10_000), CancellationToken.None);

            html = chromeBrowser.Html;
            var dontShowThisAgain = html.GetElementbyId("dontShowThisAgain");
            if (dontShowThisAgain is not null)
            {
                result = await chromeBrowser.Click(By.XPath(dontShowThisAgain.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                await new DelayClickCommand().Execute(accountId);

                var okButton = html.DocumentNode.Descendants("button").FirstOrDefault(x => x.HasClass("dialogButtonOk"));
                if (okButton is null) return Retry.ButtonNotFound("ok");
                result = await chromeBrowser.Click(By.XPath(okButton.XPath), cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            return Result.Ok();
        }

        private static HtmlNode GetSpecialUpgradeButton(HtmlDocument doc)
        {
            var node = doc.DocumentNode
                .Descendants("div")
                .FirstOrDefault(x => x.HasClass("upgradeButtonsContainer"));
            if (node is null) return null;

            var button = node
                .Descendants("button")
                .FirstOrDefault(x => x.HasClass("videoFeatureButton") && x.HasClass("green"));

            return button;
        }
    }
}