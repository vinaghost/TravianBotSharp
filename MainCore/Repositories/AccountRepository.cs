using MainCore.UI.Models.Output;

namespace MainCore.Repositories
{
    [RegisterAsTransient]
    public class AccountRepository : IAccountRepository
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        private readonly ITaskManager _taskManager;

        public AccountRepository(IDbContextFactory<AppDbContext> contextFactory, ITaskManager taskManager)
        {
            _contextFactory = contextFactory;
            _taskManager = taskManager;
        }

        public void Delete(AccountId accountId)
        {
            using var context = _contextFactory.CreateDbContext();
            context.Accounts
                .Where(x => x.Id == accountId.Value)
                .ExecuteDelete();
        }

        public List<ListBoxItem> GetItems()
        {
            using var context = _contextFactory.CreateDbContext();

            var accounts = context.Accounts
                .AsEnumerable()
                .Select(x =>
                {
                    var serverUrl = new Uri(x.Server);
                    var status = _taskManager.GetStatus(new(x.Id));
                    return new ListBoxItem()
                    {
                        Id = x.Id,
                        Color = status.GetColor(),
                        Content = $"{x.Username}{Environment.NewLine}({serverUrl.Host})"
                    };
                })
                .ToList();

            return accounts;
        }
    }
}