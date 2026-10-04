namespace MainCore.Services
{
    [RegisterScoped<IDelayService, DelayService>]
    public class DelayService(IDataService dataService, IDbContextFactory<AppDbContext> contextFactory) : IDelayService
    {
        private readonly IDataService _dataService = dataService;
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