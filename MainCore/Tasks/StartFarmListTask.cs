using MainCore.Commands.Features.StartFarmList;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class StartFarmListTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly GetSetting _getSetting;

        public StartFarmListTask(ITaskManager taskManager, GetSetting getSetting)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;

            var toFarmListPageCommand = scoped.ServiceProvider.GetRequiredService<ToFarmListPageCommand>();
            result = await toFarmListPageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var useStartAllButton = _getSetting.BooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                var startAllFarmListCommand = scoped.ServiceProvider.GetRequiredService<StartAllFarmListCommand>();
                result = await startAllFarmListCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                var startActiveFarmListCommand = scoped.ServiceProvider.GetRequiredService<StartActiveFarmListCommand>();
                result = await startActiveFarmListCommand.Execute(cancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = _getSetting.ByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}