using Humanizer;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class StartHeroFarmingTask
    {
        public sealed class Task : AccountTask
        {
            public Task(AccountId accountId) : base(accountId)
            {
            }

            protected override string TaskName => "Start hero farming";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            IChromeBrowser browser,
            ILogger logger,
            DelayClickCommand.Handler delayClickCommand,
            CancellationToken cancellationToken)
        {
            const int x = -40;
            const int y = -96;

            var heroStatus = browser.Html.DocumentNode.Descendants("div").FirstOrDefault(x => x.HasClass("heroStatus"));
            if (heroStatus is null)
            {
                logger.Warning("Hero status not found, cannot start hero farming.");
                return Result.Ok();
            }
            var result = await browser.Click(By.XPath(heroStatus.XPath));
            if (result.IsFailed)
            {
                return result;
            }

            var currentUrl = new Uri(browser.CurrentUrl);
            var host = currentUrl.GetLeftPart(UriPartial.Authority);

            await browser.Navigate($"{host}/karte.php?x={x}&y={y}", cancellationToken);

            logger.Information("Oasis at {X}|{Y}", x, y);

            result = await browser.Wait((driver) =>
            {
                var oasisElements = driver.FindElements(By.Id("tileDetails"));
                return oasisElements.Count > 0;
            }, cancellationToken);
            if (result.IsFailed)
            {
                return result;
            }
            if (!OasisParser.IsOasis(browser.Html))
            {
                logger.Warning("Not an oasis, cannot start hero farming.");
                return Result.Ok();
            }

            var typeOasis = OasisParser.GetOasisType(browser.Html);
            var animalOasis = OasisParser.GetOasisAnimal(browser.Html);

            logger.Information("Oasis type: {typeOasis}", typeOasis);
            logger.Information("Oasis animal: {animalOasis}", animalOasis);

            var simulateButton = OasisParser.GetSimulateButton(browser.Html);
            if (simulateButton is null)
            {
                logger.Warning("Simulate button not found, cannot start hero farming.");
                return Result.Ok();
            }

            result = await browser.Click(By.XPath(simulateButton.XPath));
            if (result.IsFailed)
            {
                return result;
            }
            result = await browser.WaitPageChanged("combatSimulator", cancellationToken);
            if (result.IsFailed)
            {
                return result;
            }

            result = await browser.Wait(driver =>
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return SimulatorParser.IsSimulator(html);
            }, cancellationToken);

            var troopsInput = SimulatorParser.GetAttackerInput(browser.Html);
            foreach (var troop in troopsInput)
            {
                result = await browser.Input(By.XPath(troop.XPath), "0");
                if (result.IsFailed) continue;
            }

            var simulatorButton = SimulatorParser.GetSimulateButton(browser.Html);
            if (simulatorButton is null)
            {
                logger.Warning("Simulate button not found, cannot start hero farming.");
                return Result.Ok();
            }

            result = await browser.Click(By.XPath(simulatorButton.XPath));
            if (result.IsFailed)
            {
                return result;
            }

            result = await browser.Wait(driver =>
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return SimulatorParser.GetSendTroopsButton(html) is not null;
            }, cancellationToken);

            if (result.IsFailed)
            {
                return result;
            }

            var heroHealth = SimulatorParser.GetHeroHealth(browser.Html);
            logger.Information("Hero health after battle: {heroHealth}", heroHealth);

            var rewardResource = SimulatorParser.GetRewardResource(browser.Html);
            logger.Information("Reward resource: {rewardResource}", rewardResource);

            var sendTroopButton = SimulatorParser.GetSendTroopsButton(browser.Html);
            if (sendTroopButton is null)
            {
                logger.Warning("No send troop buttons found, cannot start hero farming.");
                return Result.Ok();
            }

            await delayClickCommand.HandleAsync(new(task.AccountId), cancellationToken);
            result = await browser.Click(By.XPath(sendTroopButton.XPath));
            if (result.IsFailed)
            {
                return result;
            }

            result = await browser.WaitPageChanged("tt=2", cancellationToken);
            if (result.IsFailed)
            {
                return result;
            }

            var sendButton = RallypointParser.GetSendButton(browser.Html);
            if (sendButton is null)
            {
                logger.Warning("No send button found, cannot start hero farming.");
                return Result.Ok();
            }
            result = await browser.Click(By.XPath(sendButton.XPath));

            if (result.IsFailed)
            {
                return result;
            }

            result = await browser.Wait(driver =>
            {
                var html = new HtmlDocument();
                html.LoadHtml(driver.PageSource);
                return RallypointParser.GetConfirmButton(html) is not null;
            }, cancellationToken);

            if (result.IsFailed)
            {
                return result;
            }

            var confirmButton = RallypointParser.GetConfirmButton(browser.Html);
            if (confirmButton is null)
            {
                logger.Warning("No confirm send troops button found, cannot start hero farming.");
                return Result.Ok();
            }
            result = await browser.Click(By.XPath(confirmButton.XPath));
            if (result.IsFailed)
            {
                return result;
            }

            var waitResult = await browser.WaitPageChanged("tt=1", cancellationToken);
            if (waitResult.IsFailed)
            {
                return waitResult;
            }

            logger.Information("Hero farming started successfully.");
            var heroReturnTime = RallypointParser.GetHeroTime(browser.Html).Add(TimeSpan.FromSeconds(10)) * 2;
            logger.Information("Hero will return in: {heroTime}", heroReturnTime.Humanize(3, minUnit: Humanizer.Localisation.TimeUnit.Second));

            return Result.Ok();
        }
    }
}