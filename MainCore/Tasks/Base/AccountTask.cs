using MainCore.Tasks.Constraints;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask(AccountId accountId, DateTime executeAt) : TaskBase(executeAt), IAccountTask
    {
        public AccountId AccountId { get; } = accountId;

        public override string Description => TaskName;
    }
}