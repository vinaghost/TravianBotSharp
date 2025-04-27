using MainCore.Commands.Abstract;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.Queries
{
    [RegisterSingleton<GetAccount>]
    public class GetAccount(IDbContextFactory<AppDbContext> contextFactory, ITaskManager taskManager) : QueryBase(contextFactory)
    {
        private readonly ITaskManager _taskManager = taskManager;

        public AccountDto Execute(AccountId accountId, bool includeAccess = false)
        {
            using var context = _contextFactory.CreateDbContext();
            var query = context.Accounts
                .Where(x => x.Id == accountId.Value);

            if (includeAccess)
            {
                query = query
                    .Include(x => x.Accesses);
            }
            var account = query
                .ToDto()
                .FirstOrDefault();
            return account;
        }

        public List<ListBoxItem> Items()
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