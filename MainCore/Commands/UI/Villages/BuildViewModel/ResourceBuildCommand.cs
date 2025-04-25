using MainCore.Common.Models;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ResourceBuildCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, ResourceBuildPlan plan) : ICustomCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, plan) = command;
            await addJobCommand.HandleAsync(new(accountId, villageId, plan.ToJob(villageId)));
        }

        public static ResourceBuildPlan ToPlan(this ResourceBuildInput input)
        {
            var (type, level) = input.Get();
            return new ResourceBuildPlan()
            {
                Plan = type,
                Level = level,
            };
        }
    }
}