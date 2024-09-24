using MainCore.Commands.Features.StartAdventure;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterTransient(Registration = RegistrationStrategy.Self)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        public StartAdventureTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ToAdventurePageCommand().Execute(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ExploreAdventureCommand().Execute(AccountId, _chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var adventureDuration = GetAdventureDuration(_chromeBrowser);
            await SetNextExecute(adventureDuration);

            return Result.Ok();
        }

        private async Task SetNextExecute(TimeSpan duration)
        {
            ExecuteAt = DateTime.Now.Add(duration * 2);
            await _taskManager.ReOrder(AccountId);
        }

        private static TimeSpan GetAdventureDuration(IChromeBrowser chromeBrowser)
        {
            var html = chromeBrowser.Html;
            var heroAdventure = html.GetElementbyId("heroAdventure");
            var timer = heroAdventure
                .Descendants("span")
                .FirstOrDefault(x => x.HasClass("timer"));
            if (timer is null) return TimeSpan.Zero;

            var seconds = timer.GetAttributeValue("value", 0);
            return TimeSpan.FromSeconds(seconds);
        }

        protected override void SetName()
        {
            _name = "Start adventure";
        }
    }
}