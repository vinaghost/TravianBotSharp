namespace MainCore.Commands.Navigate
{
    public class ToQuestPageCommand : ByAccountIdBase, ICommand
    {
        public ToQuestPageCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}