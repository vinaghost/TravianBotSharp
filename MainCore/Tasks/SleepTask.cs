using MainCore.Commands.Features;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class SleepTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        public SleepTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;
            var sleepCommand = scoped.ServiceProvider.GetRequiredService<SleepCommand>();
            result = await sleepCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var accessResult = new GetAccess().Execute(AccountId);
            if (accessResult.IsFailed) return Result.Fail(accessResult.Errors).WithError(TraceMessage.Error(TraceMessage.Line()));
            var access = accessResult.Value;

            var openBrowserCommand = scoped.ServiceProvider.GetRequiredService<OpenBrowserCommand>();
            result = await openBrowserCommand.Execute(access, cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            await SetNextExecute();

            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var workTime = new GetSetting().ByName(AccountId, AccountSettingEnums.WorkTimeMin, AccountSettingEnums.WorkTimeMax);
            ExecuteAt = DateTime.Now.AddMinutes(workTime);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Sleep task";
        }
    }
}