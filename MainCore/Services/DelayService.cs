namespace MainCore.Services
{
    [RegisterScoped<DelayService>]
    public class DelayService(DataService dataService, IDbContextFactory<AppDbContext> contextFactory)
    {
        private readonly DataService _dataService = dataService;
        private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

        public async Task DelayClick(CancellationToken cancellationToken = default)
        {
            using var context = _contextFactory.CreateDbContext();
            var delay = context.ByName(_dataService.AccountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, cancellationToken);
        }

        public async Task DelayTask(CancellationToken cancellationToken = default)
        {
            using var context = _contextFactory.CreateDbContext();
            var delay = context.ByName(_dataService.AccountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}