using MainCore.Commands.Features.NpcResource;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class NpcTask
    {
        public sealed record Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId, string villageName) : base(accountId, villageId, villageName)
            {
            }

            protected override string TaskName => "NPC";
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToNpcResourcePageCommand.Handler toNpcResourcePageCommand,
            NpcResourceCommand.Handler npcResourceCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toNpcResourcePageCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            result = await npcResourceCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            return Result.Ok();
        }
    }
}