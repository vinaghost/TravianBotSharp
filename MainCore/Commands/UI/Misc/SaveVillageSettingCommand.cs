using MainCore.Commands.Base;

namespace MainCore.Commands.UI.Misc
{
    [Handler]
    public static partial class SaveVillageSettingCommand
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId, Dictionary<VillageSettingEnums, int> Settings) : ICommand;

        private static async ValueTask HandleAsync(
            Command command,
            IDbContextFactory<AppDbContext> contextFactory,
            VillageSettingUpdated.Handler villageSettingUpdated,
            CancellationToken cancellationToken
            )
        {
            var (accountId, villageId, settings) = command;
            if (settings.Count == 0) return;
            using var context = await contextFactory.CreateDbContextAsync();

            foreach (var setting in settings)
            {
                context.VillagesSetting
                    .Where(x => x.VillageId == villageId.Value)
                    .Where(x => x.Setting == setting.Key)
                    .ExecuteUpdate(x => x.SetProperty(x => x.Value, setting.Value));
            }

            await villageSettingUpdated.HandleAsync(new(accountId, villageId), cancellationToken);
        }
    }
}