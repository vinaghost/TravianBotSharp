using MainCore.Commands.Features;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<SleepTask>]
    public class SleepTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly GetAccess _getAccess;
        private readonly IGetSetting _getSetting;

        public SleepTask(ITaskManager taskManager, IGetSetting getSetting, GetAccess getAccess)
        {
            _taskManager = taskManager;
            _getSetting = getSetting;
            _getAccess = getAccess;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var sleepCommand = scoped.ServiceProvider.GetRequiredService<SleepCommand>();
            result = await sleepCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var (_, isFailed, access, errors) = await _getAccess.Execute(AccountId);
            if (isFailed) return Result.Fail(errors).WithError(TraceMessage.Error(TraceMessage.Line()));

            var openBrowserCommand = scoped.ServiceProvider.GetRequiredService<OpenBrowserCommand>();
            result = await openBrowserCommand.Execute(AccountId, access, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

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