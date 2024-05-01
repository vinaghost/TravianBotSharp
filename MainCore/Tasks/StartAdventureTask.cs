using MainCore.Commands.Features.Step.StartAdventure;
using MainCore.Tasks.Base;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ICommandHandler<ToAdventurePageCommand> _toAdventurePageCommand;
        private readonly ICommandHandler<ExploreAdventureCommand> _exploreAdventureCommand;
        private readonly ITaskManager _taskManager;
        private readonly IHeroParser _heroParser;

        public StartAdventureTask(IChromeManager chromeManager, IMediator mediator, ITaskManager taskManager, ICommandHandler<ToAdventurePageCommand> toAdventurePageCommand, ICommandHandler<ExploreAdventureCommand> exploreAdventureCommand, IHeroParser heroParser) : base(chromeManager, mediator)
        {
            _taskManager = taskManager;
            _toAdventurePageCommand = toAdventurePageCommand;
            _exploreAdventureCommand = exploreAdventureCommand;
            _heroParser = heroParser;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            var chromeBrowser = _chromeManager.Get(AccountId);

            result = await _mediator.Send(ToDorfCommand.ToDorf1(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var html = chromeBrowser.Html;

            result = await _toAdventurePageCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            result = await _exploreAdventureCommand.Handle(new(AccountId), CancellationToken);
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