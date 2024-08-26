using MainCore.Commands.Checks;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        public AccountId AccountId { get; protected set; }

        protected IChromeBrowser _chromeBrowser;
        protected IMediator _mediator;

        private readonly IChromeManager _chromeManager;

        protected AccountTask()
        {
            _chromeManager = Locator.Current.GetService<IChromeManager>();
            _mediator = Locator.Current.GetService<IMediator>();
        }

        public void Setup(AccountId accountId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            CancellationToken = cancellationToken;
        }

        protected override async Task<Result> PreExecute()
        {
            if (CancellationToken.IsCancellationRequested) return Cancel.Error;
            _chromeBrowser = _chromeManager.Get(AccountId);

            if (new IsIngamePage().Execute(_chromeBrowser))
            {
                await new UpdateAccountInfoCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
                await new UpdateVillageListCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
                return Result.Ok();
            }

            if (new IsLoginPage().Execute(_chromeBrowser))
            {
#pragma warning disable S3060 // "is" should not be used with "this"
                if (this is not LoginTask)
                {
                    ExecuteAt = ExecuteAt.AddMilliseconds(1975);
                    await _mediator.Publish(new AccountLogout(AccountId), CancellationToken);
                    return Skip.AccountLogout;
                }
#pragma warning restore S3060 // "is" should not be used with "this"
                return Result.Ok();
            }

            return Stop.NotTravianPage;
        }

        protected override async Task<Result> PostExecute()
        {
            await new UpdateAccountInfoCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            await new UpdateVillageListCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            await new CheckAdventureCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            return Result.Ok();
        }
    }
}