namespace MainCore.Commands.Update
{
    [Handler]
    public sealed partial class UpdateHeroLevelUpCommand(
        IChromeBrowser browser,
        IDbContextFactory<AppDbContext> contextFactory,
        TaskManager taskManager)
    {
        public sealed record Command(AccountId AccountId) : IAccountCommand;

        private async ValueTask HandleAsync(Command command)
        {
            await Task.CompletedTask;
            if (await browser.CurrentPage.Locator("#topBarHero i.levelUp.show").CountAsync() == 0) return;
            var accountId = command.AccountId;
            using var context = contextFactory.CreateDbContext();
            var setHeroPointTask = new SetHeroPointTask.Task(accountId);
            if (!setHeroPointTask.CanStart(context) || taskManager.IsExist<SetHeroPointTask.Task>(accountId))
            {
                return;
            }
            if (taskManager.IsExist<SetHeroPointTask.Task>(accountId)) return;
            taskManager.Add(setHeroPointTask);
        }
    }
}