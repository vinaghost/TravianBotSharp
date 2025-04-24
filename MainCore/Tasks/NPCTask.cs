using MainCore.Commands.Features.NpcResource;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<NpcTask>]
    public class NpcTask : VillageTask
    {
        private readonly StorageUpdated.Handler _storageUpdated;

        public NpcTask(StorageUpdated.Handler storageUpdated)
        {
            _storageUpdated = storageUpdated;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var toNpcResourcePageCommand = scoped.ServiceProvider.GetRequiredService<ToNpcResourcePageCommand>();
            result = await toNpcResourcePageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var npcResourceCommand = scoped.ServiceProvider.GetRequiredService<NpcResourceCommand>();
            result = await npcResourceCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateStorageCommand = scoped.ServiceProvider.GetRequiredService<UpdateStorageCommand>();
            result = await updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await _storageUpdated.HandleAsync(new(AccountId, VillageId), cancellationToken);
            return Result.Ok();
        }

        protected override string TaskName => "NPC";
    }
}