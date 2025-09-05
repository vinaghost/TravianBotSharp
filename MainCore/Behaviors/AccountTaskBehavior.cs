using MainCore.Tasks.Base;

namespace MainCore.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>
            : Behavior<TRequest, TResponse>
                where TRequest : AccountTask
                where TResponse : Result
    {
        private readonly ITaskManager _taskManager;
        private readonly IBrowser _browser;

        private readonly UpdateAccountInfoCommand.Handler _updateAccountInfoCommand;
        private readonly UpdateVillageListCommand.Handler _updateVillageListCommand;
        private readonly UpdateAdventureCommand.Handler _updateAdventureCommand;

        public AccountTaskBehavior(IBrowser browser, ITaskManager taskManager, UpdateAccountInfoCommand.Handler updateAccountInfoCommand, UpdateVillageListCommand.Handler updateVillageListCommand, UpdateAdventureCommand.Handler updateAdventureCommand)
        {
            _browser = browser;
            _taskManager = taskManager;
            _updateAccountInfoCommand = updateAccountInfoCommand;
            _updateVillageListCommand = updateVillageListCommand;
            _updateAdventureCommand = updateAdventureCommand;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            if (!LoginParser.IsIngamePage(_browser.Html))
            {
                if (!LoginParser.IsLoginPage(_browser.Html))
                {
                    // Sayfa kaybedildiğinde sayfayı yenile ve tekrar dene
                    try
                    {
                        await _browser.Refresh(cancellationToken);
                        await Task.Delay(2000, cancellationToken); // Sayfa yüklenmesi için bekle
                        
                        // Yeniden kontrol et
                        if (!LoginParser.IsIngamePage(_browser.Html) && !LoginParser.IsLoginPage(_browser.Html))
                        {
                            return (TResponse)Stop.NotTravianPage;
                        }
                    }
                    catch (Exception)
                    {
                        return (TResponse)Stop.NotTravianPage;
                    }
                }

                if (request is not LoginTask.Task)
                {
                    _taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
                    request.ExecuteAt = request.ExecuteAt.AddSeconds(1);
                    return (TResponse)Skip.AccountLogout;
                }
            }

            if (LoginParser.IsIngamePage(_browser.Html))
            {
                await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
                await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
            }

            var response = await Next(request, cancellationToken);

            if (LoginParser.IsIngamePage(_browser.Html))
            {
                await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
                await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
                await _updateAdventureCommand.HandleAsync(new(accountId), cancellationToken);
            }

            return response;
        }
    }
}
