using MainCore.Commands.Update;

namespace MainCore.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>(
        ChromeManager chromeManager,
        TaskManager taskManager,
        UpdateAccountInfoCommand.Handler updateAccountInfoCommand,
        UpdateDailyQuestCommand.Handler updateDailyQuestCommand,
        UpdateHeroLevelUpCommand.Handler updateHeroLevelUpCommand,
        UpdateAdventureCommand.Handler updateAdventureCommand)
            : Behavior<TRequest, TResponse>
                where TRequest : AccountTask
                where TResponse : Result
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return (TResponse)Cancel.Error;
            }
            var accountId = request.AccountId;
            var cacheExecuteAt = request.ExecuteAt;
            var browser = chromeManager.Get(accountId);
            var isIngamePage = await LoginParser.IsIngamePage(browser.CurrentPage);
            request.ExecuteAt = cacheExecuteAt;

            if (!isIngamePage)
            {
                var isLoginPage = await LoginParser.IsLoginPage(browser.CurrentPage);
                if (!isLoginPage)
                {
                    var result = await browser.Wait(LoginParser.GetServerTime(browser.CurrentPage));
                    if (result.IsFailed) return (TResponse)Stop.Error.WithError("Travian is not ingame nor login page. Please check browser");

                    await updateAccountInfoCommand.HandleAsync(new(accountId, browser), cancellationToken);
                }

                if (request is not LoginTask.Task)
                {
                    taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
                    request.ExecuteAt = request.ExecuteAt.AddSeconds(1);
                    return (TResponse)Skip.Error.WithError("Account is logout. Re-login now");
                }
            }
            else
            {
                await updateAccountInfoCommand.HandleAsync(new(accountId, browser), cancellationToken);
            }

            var response = await Next(request, cancellationToken);

            isIngamePage = await LoginParser.IsIngamePage(browser.CurrentPage);
            if (isIngamePage)
            {
                await updateAccountInfoCommand.HandleAsync(new(accountId, browser), cancellationToken);
                await updateAdventureCommand.HandleAsync(new(accountId, browser), cancellationToken);
                await updateDailyQuestCommand.HandleAsync(new(accountId, browser), cancellationToken);
                await updateHeroLevelUpCommand.HandleAsync(new(accountId, browser), cancellationToken);
            }

            return response;
        }
    }
}