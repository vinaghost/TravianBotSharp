using MainCore.Commands.Checks;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        public AccountId AccountId { get; protected set; }

        public void Setup(AccountId accountId)
        {
            AccountId = accountId;
        }

        protected abstract string TaskName { get; }

        public override string GetName() => TaskName;

        protected override async Task<Result> PreExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return Cancel.Error;

            var dataService = scoped.ServiceProvider.GetRequiredService<DataService>();
            dataService.AccountId = AccountId;
            var chromeManager = Locator.Current.GetService<IChromeManager>();
            var chromeBrowser = chromeManager.Get(AccountId);
            dataService.ChromeBrowser = chromeBrowser;
            var logService = Locator.Current.GetService<ILogService>();
            dataService.Logger = logService.GetLogger(AccountId);

            if (LoginParser.IsIngamePage(chromeBrowser.Html))
            {
                Result result;
                var updateAccountInfoCommand = scoped.ServiceProvider.GetRequiredService<UpdateAccountInfoCommand>();
                result = await updateAccountInfoCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                var updateVillageListCommand = scoped.ServiceProvider.GetRequiredService<UpdateVillageListCommand>();
                result = await updateVillageListCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                return Result.Ok();
            }

            if (LoginParser.IsLoginPage(chromeBrowser.Html))
            {
#pragma warning disable S3060 // "is" should not be used with "this"
                if (this is not LoginTask)
                {
                    ExecuteAt = ExecuteAt.AddMilliseconds(1975);
                    var mediator = Locator.Current.GetService<IMediator>();
                    await mediator.Publish(new AccountLogout(AccountId), cancellationToken);
                    return Skip.AccountLogout;
                }
#pragma warning restore S3060 // "is" should not be used with "this"
                return Result.Ok();
            }

            return Stop.NotTravianPage;
        }

        protected override async Task<Result> PostExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var updateAccountInfoCommand = scoped.ServiceProvider.GetRequiredService<UpdateAccountInfoCommand>();
            result = await updateAccountInfoCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateVillageListCommand = scoped.ServiceProvider.GetRequiredService<UpdateVillageListCommand>();
            result = await updateVillageListCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var checkAdventureCommand = scoped.ServiceProvider.GetRequiredService<CheckAdventureCommand>();
            result = await checkAdventureCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }
    }
}