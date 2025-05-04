using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask(AccountId accountId) : TaskBase, IAccountTask
    {
        public AccountId AccountId { get; } = accountId;

        public override string Description => TaskName;
        public override string Key => $"{AccountId}";
    }
}