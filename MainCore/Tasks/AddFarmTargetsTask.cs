using MainCore.Commands.Navigate;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class AddFarmTargetsTask(
        IChromeBrowser browser,
        ILogger logger,
        ToFarmListPageCommand.Handler toFarmListPageCommand)
    {
        public const int MaxTargetsPerList = 100;

        public sealed class Task(
            AccountId accountId,
            FarmId farmId,
            List<(int X, int Y)> targets,
            int troopSlot,
            int troopAmount) : AccountTask(accountId)
        {
            public FarmId FarmId { get; } = farmId;
            public List<(int X, int Y)> Targets { get; } = targets;
            public int TroopSlot { get; } = troopSlot;
            public int TroopAmount { get; } = troopAmount;

            protected override string TaskName => "Add farm list targets";
        }

        private async ValueTask<Result> HandleAsync(
            Task task,
            CancellationToken cancellationToken)
        {
            var result = await toFarmListPageCommand.HandleAsync(new(task.AccountId), cancellationToken);
            if (result.IsFailed) return result;

            var page = browser.CurrentPage;
            var existing = (await FarmListParser.GetTargetCoordinates(page, task.FarmId)).ToHashSet();
            var newTargets = task.Targets.Where(x => !existing.Contains(x)).Distinct().ToList();
            var freeSlots = MaxTargetsPerList - existing.Count;

            if (newTargets.Count > freeSlots)
            {
                logger.Warning("Farm list has room for {Free} more targets, skipping {Skipped} targets", freeSlots, newTargets.Count - freeSlots);
                newTargets = newTargets.Take(Math.Max(0, freeSlots)).ToList();
            }

            logger.Information("Adding {Count} targets to farm list {FarmId} ({Existing} already in the list)", newTargets.Count, task.FarmId.Value, task.Targets.Count - newTargets.Count);

            var added = 0;
            foreach (var (x, y) in newTargets)
            {
                if (cancellationToken.IsCancellationRequested) return Cancel.Error;

                result = await AddTarget(task, x, y);
                if (result.IsFailed) return result;
                if (result.Reasons.Count == 0) added++;
            }

            logger.Information("Added {Added} targets to farm list {FarmId}", added, task.FarmId.Value);
            return Result.Ok();
        }

        private async Task<Result> AddTarget(Task task, int x, int y)
        {
            var page = browser.CurrentPage;

            var result = await browser.Click(FarmListParser.GetAddTargetButton(page));
            if (result.IsFailed) return result;

            result = await browser.Wait(FarmListParser.GetTargetForm(page));
            if (result.IsFailed) return result;

            try
            {
                await FarmListParser.GetTargetListSelect(page).SelectOptionAsync(task.FarmId.Value.ToString());
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Cannot select farm list {task.FarmId.Value}. Details: {ex.Message}");
            }

            result = await browser.Input(FarmListParser.GetTargetXInput(page), x.ToString());
            if (result.IsFailed) return result;

            result = await browser.Input(FarmListParser.GetTargetYInput(page), y.ToString());
            if (result.IsFailed) return result;

            result = await browser.Input(FarmListParser.GetTargetTroopInput(page, task.TroopSlot), task.TroopAmount.ToString());
            if (result.IsFailed) return result;

            // the game looks the target up after the coordinates are typed and only then enables Save
            var saveButton = FarmListParser.GetTargetSaveButton(page);
            try
            {
                await page.Locator("#farmListTargetForm button.save:not([disabled])").WaitForAsync(new() { Timeout = 10_000 });
            }
            catch (TimeoutException)
            {
                logger.Warning("Target ({X}|{Y}) was not accepted by the game, skipping it", x, y);
                result = await browser.Click(FarmListParser.GetTargetCancelButton(page));
                if (result.IsFailed) return result;
                return Result.Ok().WithReason(new Success("skipped"));
            }

            result = await browser.Click(saveButton);
            if (result.IsFailed) return result;

            try
            {
                await FarmListParser.GetTargetForm(page).WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = 10_000 });
            }
            catch (TimeoutException ex)
            {
                return Retry.Error.WithError($"Add target dialog did not close after saving ({x}|{y}). Details: {ex.Message}");
            }

            logger.Information("Added ({X}|{Y}) to farm list", x, y);
            return Result.Ok();
        }
    }
}
