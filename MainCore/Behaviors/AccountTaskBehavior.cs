using MainCore.Tasks.Base;

namespace MainCore.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>
            : Behavior<TRequest, TResponse>
                where TRequest : AccountTask
                where TResponse : Result
    {
        private readonly ITaskManager _taskManager;
        private readonly IChromeBrowser _browser;

        private readonly UpdateAccountInfoCommand.Handler _updateAccountInfoCommand;
        private readonly UpdateVillageListCommand.Handler _updateVillageListCommand;
        private readonly UpdateAdventureCommand.Handler _updateAdventureCommand;

        public AccountTaskBehavior(IChromeBrowser browser, ITaskManager taskManager, UpdateAccountInfoCommand.Handler updateAccountInfoCommand, UpdateVillageListCommand.Handler updateVillageListCommand, UpdateAdventureCommand.Handler updateAdventureCommand)
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
            var html = _browser.Html;
            if (!LoginParser.IsIngamePage(html))
            {
                if (!LoginParser.IsLoginPage(html))
                {
                    return (TResponse)Stop.NotTravianPage;
                }

                if (request is not LoginTask.Task)
                {
                    _taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
                    return (TResponse)Skip.AccountLogout;
                }
            }

            await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);

            await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);

            var response = await Next(request, cancellationToken);

            await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);

            await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);

            await _updateAdventureCommand.HandleAsync(new(accountId), cancellationToken);
            return response;
        }
    }
}