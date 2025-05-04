using MainCore.Commands.Features.NpcResource;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class NpcTask
    {
        public sealed class Task : VillageTask
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
            UpdateStorageCommand.Handler updateStorageCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toNpcResourcePageCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            result = await npcResourceCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            await updateStorageCommand.HandleAsync(new(task.AccountId, task.VillageId), cancellationToken);
            return Result.Ok();
        }
    }
}