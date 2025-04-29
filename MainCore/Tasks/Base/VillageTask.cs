using MainCore.Commands.Checks;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
        public VillageId VillageId { get; protected set; }

        protected string VillageName { get; private set; }

        public override string GetName() => $"{TaskName} in {VillageName}";

        public async Task Setup(AccountId accountId, VillageId villageId)
        {
            AccountId = accountId;
            VillageId = villageId;

            var getVillageNameQuery = Locator.Current.GetService<GetVillageNameQuery.Handler>();
            VillageName = await getVillageNameQuery.HandleAsync(new(villageId));
        }

        protected override async Task<Result> PreExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            result = await base.PreExecute(scoped, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var dataService = scoped.ServiceProvider.GetRequiredService<IDataService>();
            dataService.VillageId = VillageId;

            var switchVillageCommand = scoped.ServiceProvider.GetRequiredService<SwitchVillageCommand>();
            result = await switchVillageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateStorageCommand = scoped.ServiceProvider.GetRequiredService<UpdateStorageCommand>();
            result = await updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override async Task<Result> PostExecute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;

            result = await base.PostExecute(scoped, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var toDorfCommand = scoped.ServiceProvider.GetRequiredService<ToDorfCommand>();
            result = await toDorfCommand.Execute(0, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateBuildingCommand = scoped.ServiceProvider.GetRequiredService<UpdateBuildingCommand>();
            result = await updateBuildingCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var updateStorageCommand = scoped.ServiceProvider.GetRequiredService<UpdateStorageCommand>();
            result = await updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var checkQuestCommand = scoped.ServiceProvider.GetRequiredService<CheckQuestCommand.Handler>();
            await checkQuestCommand.HandleAsync(new(AccountId, VillageId), cancellationToken);
            return Result.Ok();
        }
    }
}