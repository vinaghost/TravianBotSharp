namespace MainCore.Commands.UI.Villages.BuildViewModel
{
    [Handler]
    public static partial class DeleteJobByVillageIdCommand
    {
        public sealed record Command(VillageId VillageId) : IVillageCommand;

        private static async ValueTask HandleAsync(
            Command command,
            AppDbContext context,
            CancellationToken cancellationToken
            )
        {
            await Task.CompletedTask;
            context.Jobs
                .Where(x => x.VillageId == command.VillageId.Value)
                .ExecuteDelete();
        }
    }
}