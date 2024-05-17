using MainCore.Commands.Features.DisableRedRaidReport;
using MainCore.Commands.Features.StartFarmList;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTask]
    public class StartFarmListTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        public StartFarmListTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            if (IsDisableRedRaidReport() && IsNewReport())
            {
                result = await new ToRedRaidReportPageCommand().Execute(_chromeBrowser, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await new DisableRedRaidCommand().Execute(_chromeBrowser, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await new ToReportPageCommand().Execute(_chromeBrowser, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

                result = await new MarkAllAsReadCommand().Execute(_chromeBrowser, CancellationToken);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }

            result = await new ToFarmListPageCommand().Execute(_chromeBrowser, AccountId, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var useStartAllButton = new GetSetting().BooleanByName(AccountId, AccountSettingEnums.UseStartAllButton);
            if (useStartAllButton)
            {
                result = await new StartAllFarmListCommand().Execute(_chromeBrowser);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            else
            {
                result = await new StartActiveFarmListCommand().Execute(_chromeBrowser, AccountId);
                if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));
            }
            await SetNextExecute();
            return Result.Ok();
        }

        private async Task SetNextExecute()
        {
            var seconds = new GetSetting().ByName(AccountId, AccountSettingEnums.FarmIntervalMin, AccountSettingEnums.FarmIntervalMax);
            ExecuteAt = DateTime.Now.AddSeconds(seconds);
            await _taskManager.ReOrder(AccountId);
        }

        private bool IsDisableRedRaidReport()
        {
            return new GetSetting().BooleanByName(AccountId, AccountSettingEnums.EnableAutoDisableRedRaidReport);
        }

        private bool IsNewReport()
        {
            var html = _chromeBrowser.Html;

            var navigation = html.GetElementbyId("navigation");
            if (navigation is null) return false;
            var reports = navigation.Descendants("a").FirstOrDefault(x => x.HasClass("reports"));
            if (reports is null) return false;
            return reports.Descendants("div").Any(x => x.HasClass("indicator"));
        }

        protected override void SetName()
        {
            _name = "Start farm list";
        }
    }
}