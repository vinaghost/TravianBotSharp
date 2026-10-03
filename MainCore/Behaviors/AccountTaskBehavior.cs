using MainCore.Tasks.Base;

namespace MainCore.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>(
        IChromeBrowser browser,
        ITaskManager taskManager,
        UpdateAccountInfoCommand.Handler updateAccountInfoCommand,
        UpdateVillageListCommand.Handler updateVillageListCommand,
        UpdateAdventureCommand.Handler updateAdventureCommand)
            : Behavior<TRequest, TResponse>
                where TRequest : AccountTask
                where TResponse : Result
    {
        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;

            var isIngamePage = await LoginParser.IsIngamePage(browser.CurrentPage);
            if (!isIngamePage)
            {
                var isLoginPage = await LoginParser.IsLoginPage(browser.CurrentPage);
                if (!isLoginPage)
                {
                    var result = await browser.Wait(LoginParser.GetServerTime(browser.CurrentPage));
                    if (result.IsFailed) return (TResponse)Stop.Error.WithError("Travian is not ingame nor login page. Please check browser");

                    await updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
                    await updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
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
                await updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
                await updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
            }

            var response = await Next(request, cancellationToken);

            isIngamePage = await LoginParser.IsIngamePage(browser.CurrentPage);
            if (isIngamePage)
            {
                await updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
                await updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
                await updateAdventureCommand.HandleAsync(new(accountId), cancellationToken);
            }

            return response;
        }
    }
}