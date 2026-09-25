using MainCore.Commands.NextExecute;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public sealed partial class StartAdventureTask(IChromeBrowser browser,
                                                   ILogger logger)
    {
        public sealed class Task(AccountId accountId) : AccountTask(accountId)
        {
            protected override string TaskName => "Start adventure";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(AccountId, AccountSettingEnums.EnableAutoStartAdventure);
                if (!settingEnable) return false;

                return true;
            }
        }

        private async ValueTask<Result> HandleAsync(Task task)
        {
            Result result;
            result = await ToAdventurePage();
            if (result.IsFailed) return result;

            var canStartAdventure = await AdventureParser.CanStartAdventure(browser.CurrentPage);
            if (!canStartAdventure) return Skip.Error.WithError("No adventure available");

            result = await ExploreAdeventure();
            if (result.IsFailed) return result;

            var adventureDuration = await AdventureParser.GetAdventureDuration(browser.CurrentPage);
            task.ExecuteAt = DateTime.Now.Add(adventureDuration * 2);

            return Result.Ok();
        }

        private async Task<Result> ExploreAdeventure()
        {
            var adventures = await AdventureParser.GetAdventureInfo(browser.CurrentPage);
            if (adventures.Count == 0) return Skip.Error.WithError("No adventure available");

            var adventure = adventures[0];

            logger.Information("Start {Difficult} adventure takes {Duration} ", adventure.Difficult, adventure.Duration);

            var result = await browser.Click(adventure.Button);
            if (result.IsFailed) return result;

            result = await browser.Wait(AdventureParser.GetContinueButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }

        private async Task<Result> ToAdventurePage()
        {
            var result = await browser.Click(AdventureParser.GetHeroAdventureButton(browser.CurrentPage));
            if (result.IsFailed) return result;
            result = await browser.WaitPageChanged("hero/adventures");
            if (result.IsFailed) return result;
            result = await browser.Wait(AdventureParser.GetAdventurePage(browser.CurrentPage));
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}