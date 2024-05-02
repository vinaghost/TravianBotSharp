namespace MainCore.Commands.General
{
    public class CloseBrowserCommand : ByAccountIdBase, ICommand
    {
        public CloseBrowserCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    [RegisterAsTransient]
    public class CloseBrowserCommandHandler : ICommandHandler<CloseBrowserCommand>
    {
        private readonly IChromeManager _chromeManager;

        public CloseBrowserCommandHandler(IChromeManager chromeManager)
        {
            _chromeManager = chromeManager;
        }

        public async Task<Result> Handle(CloseBrowserCommand command, CancellationToken cancellationToken)
        {
            var chromeBrowser = _chromeManager.Get(command.AccountId);
            await Task.Run(chromeBrowser.Close, CancellationToken.None);
            return Result.Ok();
        }
    }
}