using MainCore.Commands.Features.NpcResource;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class NpcTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "NPC";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoNPCEnable);
                if (!settingEnable) return false;

                var gold = context.AccountsInfo
                    .Where(x => x.AccountId == AccountId.Value)
                    .Select(x => x.Gold)
                    .FirstOrDefault();
                if (gold < 3) return false;

                var granaryPercent = (int)context.Storages
                   .Where(x => x.VillageId == VillageId.Value)
                   .Select(x => x.Crop * 100f / x.Granary)
                   .FirstOrDefault();

                var autoNPCGranaryPercent = context.ByName(VillageId, VillageSettingEnums.AutoNPCGranaryPercent);
                if (granaryPercent < autoNPCGranaryPercent) return false;

                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToNpcResourcePageCommand.Handler toNpcResourcePageCommand,
            NpcResourceCommand.Handler npcResourceCommand,
            CancellationToken cancellationToken)
        {
            Result result;
            result = await toNpcResourcePageCommand.HandleAsync(new(task.VillageId), cancellationToken);
            if (result.IsFailed) return result;
            result = await npcResourceCommand.HandleAsync(new(task.VillageId), cancellationToken);
            if (result.IsFailed)
            {
                if (result.HasError<StorageLimit>())
                {
                    task.ExecuteAt = DateTime.Now.AddHours(5);
                    return new Skip();
                }
                return result;
            }
            return Result.Ok();
        }
    }
}