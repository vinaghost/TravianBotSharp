using MainCore.Commands.Checks;
using MainCore.Tasks.Base;

namespace MainCore.Tasks.Behaviors
{
    public sealed class AccountTaskBehavior<TRequest, TResponse>
            : Behavior<TRequest, TResponse>
                where TRequest : IAccountTask
                where TResponse : Result
    {
        private readonly IChromeManager _chromeManager;
        private readonly ITaskManager _taskManager;

        private readonly UpdateAccountInfoCommand.Handler _updateAccountInfoCommand;
        private readonly UpdateVillageListCommand.Handler _updateVillageListCommand;
        private readonly CheckAdventureCommand.Handler _checkAdventureCommand;

        public AccountTaskBehavior(IChromeManager chromeManager, ITaskManager taskManager, UpdateAccountInfoCommand.Handler updateAccountInfoCommand, UpdateVillageListCommand.Handler updateVillageListCommand, CheckAdventureCommand.Handler checkAdventureCommand)
        {
            _chromeManager = chromeManager;
            _taskManager = taskManager;
            _updateAccountInfoCommand = updateAccountInfoCommand;
            _updateVillageListCommand = updateVillageListCommand;
            _checkAdventureCommand = checkAdventureCommand;
        }

        public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            var browser = _chromeManager.Get(accountId);
            var html = browser.Html;
            if (!LoginParser.IsIngamePage(html))
            {
                if (!LoginParser.IsLoginPage(html))
                {
                    return (TResponse)Stop.NotTravianPage;
                }

                if (request is not LoginTask.Task)
                {
                    await _taskManager.AddOrUpdate<LoginTask.Task>(new(accountId), first: true);
                    return (TResponse)Skip.AccountLogout;
                }
            }

            await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
            await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);

            var response = await Next(request, cancellationToken);

            await _updateAccountInfoCommand.HandleAsync(new(accountId), cancellationToken);
            await _updateVillageListCommand.HandleAsync(new(accountId), cancellationToken);
            await _checkAdventureCommand.HandleAsync(new(accountId), cancellationToken);
            return response;
        }
    }
}