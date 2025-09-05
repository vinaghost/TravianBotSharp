namespace MainCore.Tasks.Base
{
    public abstract class AccountTask(AccountId accountId) : BaseTask, IAccountConstraint
    {
        public AccountId AccountId { get; } = accountId;

        public override string Description => TaskName;
        public override string Key => $"{AccountId}";
    }
}
