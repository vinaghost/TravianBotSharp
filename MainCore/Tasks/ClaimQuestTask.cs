#pragma warning disable S1172

using MainCore.Commands.Features.ClaimQuest;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [Handler]
    public static partial class ClaimQuestTask
    {
        public sealed class Task : VillageTask
        {
            public Task(AccountId accountId, VillageId villageId) : base(accountId, villageId)
            {
            }

            protected override string TaskName => "Claim quest";

            public override bool CanStart(AppDbContext context)
            {
                var settingEnable = context.BooleanByName(VillageId, VillageSettingEnums.AutoClaimQuestEnable);
                if (!settingEnable) return false;

                return true;
            }
        }

        private static async ValueTask<Result> HandleAsync(
            Task task,
            ToQuestPageCommand.Handler toQuestPageCommand,
            ClaimQuestCommand.Handler claimQuestCommand,
            CancellationToken cancellationToken)
        {
            try
            {
                Result result;
                result = await toQuestPageCommand.HandleAsync(new(), cancellationToken);
                if (result.IsFailed) 
                {
                    // Quest sayfasýna gidemiyor ise görev iptal edilir (quest yoktur)
                    return Result.Ok(); 
                }
                
                result = await claimQuestCommand.HandleAsync(new(), cancellationToken);
                if (result.IsFailed) 
                {
                    // Quest claim edilemiyor ise görev iptal edilir (claim edilecek quest yoktur)
                    return Result.Ok();
                }
                
                return Result.Ok();
            }
            catch (Exception)
            {
                // Herhangi bir exception durumunda task iptal edilir (timeout, network vs.)
                return Result.Ok();
            }
        }
    }
}
