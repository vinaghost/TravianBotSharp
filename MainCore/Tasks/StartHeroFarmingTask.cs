using Humanizer;
using MainCore.Commands.Features.StartHeroFarming;
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
            GetTargetCommand.Handler getTargetCommand,
            ValidateHeroHeathCommand.Handler validateHeroHeathCommand,
            ValidateRewardResourceCommand.Handler validateRewardResourceCommand,
            UpdateTargetCommand.Handler updateTargetCommand,
            DeleteTargetCommand.Handler deleteTargetCommand,
            CancellationToken cancellationToken)
        {
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

            do
            {
                var target = await getTargetCommand.HandleAsync(new(task.AccountId), cancellationToken);
                if (target is null)
                {
                    logger.Warning("Target not found, cannot start hero farming.");
                    return Result.Ok();
                }

                if (target.LastSend > DateTime.Now.AddHours(-1))
                {
                    logger.Information("Hero farming already started, waiting for next target.");
                    task.ExecuteAt = DateTime.Now.AddHours(1);
                    return Result.Ok();
                }
                logger.Information("Open map at {X}|{Y}", target.X, target.Y);

                var currentUrl = new Uri(browser.CurrentUrl);
                var host = currentUrl.GetLeftPart(UriPartial.Authority);
                await browser.Navigate($"{host}/karte.php?x={target.X}&y={target.Y}", cancellationToken);

                result = await browser.Wait((driver) =>
                {
                    var oasisElements = driver.FindElements(By.Id("tileDetails"));
                    return oasisElements.Count > 0;
                }, cancellationToken);

                if (result.IsFailed)
                {
                    logger.Warning("Load data failed, cannot start hero farming.", target.X, target.Y);
                    return result;
                }
                if (!OasisParser.IsOasis(browser.Html))
                {
                    logger.Warning("Oasis not found at {X}|{Y}, delete this target.", target.X, target.Y);
                    await deleteTargetCommand.HandleAsync(new(task.AccountId, target), cancellationToken);
                    continue;
                }

                target.OasisType = OasisParser.GetOasisType(browser.Html);
                target.Animal = OasisParser.GetOasisAnimal(browser.Html);

                logger.Information("Oasis type: {typeOasis}", target.OasisType);
                logger.Information("Oasis animal: {animalOasis}", target.Animal);

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
                    logger.Warning("Failed to load combat simulator page, cannot start hero farming.");
                    return result;
                }

                result = await browser.Wait(driver =>
                {
                    var html = new HtmlDocument();
                    html.LoadHtml(driver.PageSource);
                    return SimulatorParser.IsSimulator(html);
                }, cancellationToken);
                if (result.IsFailed)
                {
                    logger.Warning("Combat simulator not found, cannot start hero farming.");
                    return result;
                }

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
                    logger.Warning("Failed to load send troops button, cannot start hero farming.");
                    return result;
                }

                var heroHealth = SimulatorParser.GetHeroHealth(browser.Html);
                var rewardResource = SimulatorParser.GetRewardResource(browser.Html);

                target.Resource = rewardResource;
                target.LastSend = DateTime.Now;

                await updateTargetCommand.HandleAsync(new(task.AccountId), cancellationToken);

                if (!(await validateHeroHeathCommand.HandleAsync(new(task.AccountId, heroHealth), cancellationToken)))
                {
                    task.ExecuteAt = DateTime.Now.AddHours(1);
                    return Result.Ok();
                }

                if (!(await validateRewardResourceCommand.HandleAsync(new(task.AccountId, rewardResource), cancellationToken)))
                {
                    logger.Warning("Ignore this target.");
                    continue;
                }

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
                    logger.Warning("Failed to load send troops page, cannot start hero farming.");
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
                    logger.Warning("Failed to load confirm send troops button, cannot start hero farming.");
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
                    logger.Warning("Failed to load rally point page after sending troops, cannot start hero farming.");
                    return waitResult;
                }

                logger.Information("Hero farming started successfully.");
                var heroReturnTime = RallypointParser.GetHeroTime(browser.Html).Add(TimeSpan.FromSeconds(10)) * 2;
                logger.Information("Hero will return in: {heroTime}", heroReturnTime.Humanize(3, minUnit: Humanizer.Localisation.TimeUnit.Second));
                return Result.Ok();
            }
            while (true);
        }
    }
}