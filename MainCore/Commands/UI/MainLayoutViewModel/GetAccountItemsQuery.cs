using MainCore.Constraints;
using MainCore.UI.Models.Output;

namespace MainCore.Commands.UI.MainLayoutViewModel
{
    [Handler]
    public static partial class GetAccountItemsQuery
    {
        public sealed record Query() : IQuery;

        private static async ValueTask<List<ListBoxItem>> HandleAsync(
            Query query,
            AppDbContext context, ITaskManager taskManager,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;

            var items = context.Accounts
                .AsEnumerable()
                .Select(x =>
                {
                    var serverUrl = new Uri(x.Server);
                    var status = taskManager.GetStatus(new(x.Id));
                    return new ListBoxItem()
                    {
                        Id = x.Id,
                        Color = status.GetColor(),
                        Content = $"{x.Username}{Environment.NewLine}({serverUrl.Host})"
                    };
                })
                .ToList();

            return items;
        }
    }
}