namespace MainCore.Commands.UI.DebugViewModel
{
    [Handler]
    public static partial class GetEndpointAdressQuery
    {
        public sealed record Query(AccountId AccountId) : ICustomQuery;
        private const string NotOpen = "Chrome didn't open yet";

        private static async ValueTask<string> HandleAsync(
            Query query,
            ITaskManager taskManager,
            CancellationToken token
        )
        {
            await Task.CompletedTask;
            var status = taskManager.GetStatus(query.AccountId);
            if (status == StatusEnums.Offline) return NotOpen;
            var address = "address enpoint is disabled";
            if (string.IsNullOrEmpty(address)) return NotOpen;
            return address;
        }
    }
}