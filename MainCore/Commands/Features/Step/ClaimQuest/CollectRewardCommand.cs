namespace MainCore.Commands.Features.Step.ClaimQuest
{
    public class CollectRewardCommand : ByAccountIdBase, ICommand
    {
        public CollectRewardCommand(AccountId accountId) : base(accountId)
        {
        }
    }
}