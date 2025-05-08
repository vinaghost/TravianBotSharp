using MainCore.Constraints;
using MainCore.UI.Models.Input;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ResourceBuildCommand
    {
        public sealed record Command(VillageId VillageId, ResourceBuildPlan plan) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand,
            CancellationToken cancellationToken
            )
        {
            var (villageId, plan) = command;
            await addJobCommand.HandleAsync(new(villageId, plan.ToJob(villageId)));
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