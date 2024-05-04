using MainCore.Commands.Features.StartAdventure;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        public StartAdventureTask(IMediator mediator, ITaskManager taskManager) : base(mediator)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var html = _chromeBrowser.Html;

            result = await new ToAdventurePageCommand().Execute(_chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await new ExploreAdventureCommand().Execute(AccountId, _chromeBrowser, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var adventureDuration = new GetAdventureDuration().Execute(_chromeBrowser);
            await SetNextExecute(adventureDuration);

            return Result.Ok();
        }

        private async Task SetNextExecute(TimeSpan duration)
        {
            ExecuteAt = DateTime.Now.Add(duration * 2);
            await _taskManager.ReOrder(AccountId);
        }

        protected override void SetName()
        {
            _name = "Start adventure";
        }
    }
}