namespace MainCore.Commands.Navigate
{
    public class ToBuildingCommand : ByAccountIdBase, ICommand
    {
        public int Location { get; }

        public ToBuildingCommand(AccountId accountId, int location) : base(accountId)
        {
            Location = location;
        }
    }
}