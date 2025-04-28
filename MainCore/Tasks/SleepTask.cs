using MainCore.Commands.Features;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<SleepTask>]
    public class SleepTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly GetAccessQuery.Handler _getAccessQuery;
        private readonly IGetSetting _getSetting;

        public SleepTask(ITaskManager taskManager, IGetSetting getSetting, GetAccessQuery.Handler getAccessQuery)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
            _getAccessQuery = getAccessQuery;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var sleepCommand = scoped.ServiceProvider.GetRequiredService<SleepCommand>();
            result = await sleepCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var (_, isFailed, access, errors) = await _getAccessQuery.HandleAsync(new(AccountId), cancellationToken);
            if (isFailed) return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));

            var openBrowserCommand = scoped.ServiceProvider.GetRequiredService<OpenBrowserCommand.Handler>();
            await openBrowserCommand.HandleAsync(new(AccountId, access), cancellationToken);

            await SetNextExecute();

            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var workTime = _getSetting.ByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await _taskManager.ReOrder(AccountId);
        }

        protected override string TaskName => "Sleep";
    }
}