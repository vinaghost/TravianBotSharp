using MainCore.Commands.Features.StartAdventure;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ITaskManager _taskManager;
        private readonly IHeroParser _heroParser;

        public StartAdventureTask(IChromeManager chromeManager, IMediator mediator, ITaskManager taskManager, IHeroParser heroParser) : base(chromeManager, mediator)
        {
            _taskManager = taskManager;
            _heroParser = heroParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            var chromeBrowser = _chromeManager.Get(AccountId);

            result = await new ToDorfCommand().Execute(_chromeBrowser, 0, false, CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var html = chromeBrowser.Html;

            if (!_heroParser.CanStartAdventure(html)) return Result.Ok();

            result = await _mediator.Send(new ToAdventurePageCommand(chromeBrowser), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _mediator.Send(new ExploreAdventureCommand(chromeBrowser), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var adventureDuration = _heroParser.GetAdventureDuration(html);

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