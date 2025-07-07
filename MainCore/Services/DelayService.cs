namespace MainCore.Services
{
    [RegisterScoped<IDelayService, DelayService>]
    public class DelayService : IDelayService
    {
        private readonly AppDbContext _context;
        private readonly DataService _dataService;

        public DelayService(AppDbContext context, DataService dataService)
        {
            _context = context;
            _dataService = dataService;
        }

        public async Task DelayClick(CancellationToken cancellationToken = default)
        {
            var delay = _context.ByName(_dataService.AccountId, AccountSettingEnums.ClickDelayMin, AccountSettingEnums.ClickDelayMax);
            await Task.Delay(delay, cancellationToken);
        }

        public async Task DelayTask(CancellationToken cancellationToken = default)
        {
            var delay = _context.ByName(_dataService.AccountId, AccountSettingEnums.TaskDelayMin, AccountSettingEnums.TaskDelayMax);
            await Task.Delay(delay, cancellationToken);
        }
    }
}