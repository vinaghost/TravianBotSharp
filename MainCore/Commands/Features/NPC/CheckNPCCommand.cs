namespace MainCore.Commands.Features.NPC
{
    [Handler]
    public sealed partial class CheckNPCCommand(
        ChromeManager chromeManager,
        IDbContextFactory<AppDbContext> contextFactory,
        ILogger logger)
    {
        public sealed record Command(AccountId AccountId, VillageId VillageId) : IAccountVillageCommand
        {
            public string Name => $"{nameof(CheckNPCCommand)}";
        }

        private async ValueTask<bool> HandleAsync(Command command)
        {
            var browser = chromeManager.Get(command.AccountId);

            var storage = await StorageParser.GetStorage(browser.CurrentPage);

            var granaryPercent = (int)(storage.Crop * 100f / storage.Granary);

            using var context = contextFactory.CreateDbContext();
            var autoNPCGranaryPercent = context.ByName(command.VillageId, VillageSettingEnums.AutoNPCGranaryPercent);

            if (granaryPercent < autoNPCGranaryPercent)
            {
                logger.Information("NPC resources not available. Granary percent is too low: {GranaryPercent} < {AutoNPCGranaryPercent}",
                    granaryPercent, autoNPCGranaryPercent);
                return false;
            }

            return true;
        }
    }
}