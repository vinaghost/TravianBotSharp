namespace MainCore.Commands.Navigate
{
    public class ToAdventurePageCommand : ByAccountIdBase, ICommand
    {
        public ToAdventurePageCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}