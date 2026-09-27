using MainCore.UI.Models.Input;
using System.Text.Json;

namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class ResourceBuildCommand
    {
        public sealed record Command(VillageId VillageId, ResourceBuildPlan plan) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AddJobCommand.Handler addJobCommand
            )
        {
            var (villageId, plan) = command;

            var job = new JobDto()
            {
                Position = 0,
                Type = JobTypeEnums.ResourceBuild,
                Content = JsonSerializer.Serialize(plan),
            };
            await addJobCommand.HandleAsync(new(villageId, job));
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