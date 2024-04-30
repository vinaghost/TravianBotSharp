using MainCore.Commands.Misc;
using Splat;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        protected AccountTask(IChromeManager chromeManager, UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(chromeManager, unitOfCommand, unitOfRepository, mediator)
        {
        }

        public AccountId AccountId { get; protected set; }

        private INavigationBarParser _navigationBarParser;
        private ILoginPageParser _loginPageParser;

        public void Setup(AccountId accountId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            CancellationToken = cancellationToken;
        }

        protected override async Task<Result> PreExecute()
        {
            if (CancellationToken.IsCancellationRequested) return Cancel.Error;

            _navigationBarParser ??= Locator.Current.GetService<INavigationBarParser>();

            if (IsIngame())
            {
                Result result;
                result = await _mediator.Send(new UpdateAccountInfoCommand(AccountId));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                result = await _mediator.Send(new UpdateVillageListCommand(AccountId));
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
                return Result.Ok();
            }

            _loginPageParser ??= Locator.Current.GetService<ILoginPageParser>();

            if (IsLogin())
            {
                if (this is not LoginTask)
                {
                    ExecuteAt = ExecuteAt.AddMilliseconds(1975);
                    await _mediator.Publish(new AccountLogout(AccountId), CancellationToken);
                    return Skip.AccountLogout;
                }
                return Result.Ok();
            }

            return Stop.NotTravianPage;
        }

        protected override async Task<Result> PostExecute()
        {
            Result result;
            result = await _mediator.Send(new CheckAdventureCommand(AccountId));
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            return Result.Ok();
        }

        private bool IsIngame()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var fieldButton = _navigationBarParser.GetResourceButton(html);

            return fieldButton is not null;
        }

        private bool IsLogin()
        {
            var chromeBrowser = _chromeManager.Get(AccountId);
            var html = chromeBrowser.Html;

            var loginButton = _loginPageParser.GetLoginButton(html);

            return loginButton is not null;
        }
    }
}