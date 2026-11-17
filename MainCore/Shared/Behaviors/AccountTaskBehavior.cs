using MainCore.Features.Login;
using MainCore.Features.Update;

namespace MainCore.Shared.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>(
        ILogger logger,
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
            var isIngamePage = await browser.CurrentPage.IsIngamePage();
            request.ExecuteAt = cacheExecuteAt;

            if (!isIngamePage)
            {
                var isLoginPage = await browser.CurrentPage.IsLoginPage();
                if (!isLoginPage)
                {
                    try
                    {
                        await browser.CurrentPage.GetServerTime().WaitForAsync();
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Travian is not ingame nor login page. Please check browser. Exception: {Exception}", ex.Message);
                        return (TResponse)Stop.Error;
                    }

                    await updateAccountInfoCommand.HandleAsync(new(browser), cancellationToken);
                }

                if (request is not LoginTask.Task)
                {
                    taskManager.Add<LoginTask.Task>(new(accountId), first: true);
                    request.ExecuteAt = request.ExecuteAt.AddSeconds(1);
                    logger.Information("Account is logout. Re-login now");
                    return (TResponse)Skip.Error;
                }
            }
            else
            {
                await updateAccountInfoCommand.HandleAsync(new(browser), cancellationToken);
            }

            var response = await Next(request, cancellationToken);

            isIngamePage = await browser.CurrentPage.IsIngamePage();
            if (isIngamePage)
            {
                await updateAccountInfoCommand.HandleAsync(new(browser), cancellationToken);
                await updateAdventureCommand.HandleAsync(new(browser), cancellationToken);
                await updateDailyQuestCommand.HandleAsync(new(browser), cancellationToken);
                await updateHeroLevelUpCommand.HandleAsync(new(browser), cancellationToken);
            }

            return response;
        }
    }
}