using MainCore.Constraints;
using MainCore.Notifications.Behaviors;

namespace MainCore.Commands.UI.Misc
{
    [Handler]
    [Behaviors(typeof(VillageSettingUpdatedBehavior<,>))]
    public static partial class SaveVillageSettingCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, Dictionary<VillageSettingEnums, int> Settings) : IAccountVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            var (accountId, villageId, settings) = command;
            if (settings.Count == 0) return;

            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }
        }
    }
}