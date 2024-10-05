using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class UpdateVillageTask : VillageTask
    {
        private readonly ITaskManager _taskManager;

        public UpdateVillageTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            var dataService = scoped.ServiceProvider.GetRequiredService<DataService>();
            var chromeBrowser = dataService.ChromeBrowser;
            var url = chromeBrowser.CurrentUrl;
            Result result;

            await chromeBrowser.Refresh(cancellationToken);

            var updateBuildingCommand = scoped.ServiceProvider.GetRequiredService<UpdateBuildingCommand>();
            var toDorfCommand = scoped.ServiceProvider.GetRequiredService<ToDorfCommand>();

            if (url.Contains("dorf1"))
            {
                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else if (url.Contains("dorf2"))
            {
                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await toDorfCommand.Execute(1, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await toDorfCommand.Execute(1, cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await updateBuildingCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            var updateStorageCommand = scoped.ServiceProvider.GetRequiredService<UpdateStorageCommand>();
            result = await updateStorageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = Locator.Current.GetService<GetSetting>().ByName(VillageId, VillageSettingEnums.AutoRefreshMin, VillageSettingEnums.AutoRefreshMax, 60);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            var village = Locator.Current.GetService<GetVillageName>().Execute(VillageId);
            _name = $"Update village in {village}";
        }
    }
}