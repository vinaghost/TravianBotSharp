using MainCore.Commands.UI.Villages.AttackViewModel;
using MainCore.Models;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class AttackTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName, AttackPlan plan, DateTime confirmAt) : base(accountId, villageId, villageName)
            {
                Plan = plan;
                ConfirmAt = confirmAt;
            }

            protected override string TaskName => "Send attack";

            public AttackPlan Plan { get; }

            public DateTime ConfirmAt { get; }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            SendAttackCommand.Handler sendAttackCommand,
            CancellationToken cancellationToken)
        {
            var result = await sendAttackCommand.HandleAsync(new(task.AccountId, task.VillageId, task.Plan, task.ConfirmAt), cancellationToken);
            return result;
        }
    }
}
