using MainCore.Commands.Features.StartAdventure;
using MainCore.Tasks.Base;
using Microsoft.Extensions.DependencyInjection;

namespace MainCore.Tasks
{
    [RegisterTransient<StartAdventureTask>]
    public class StartAdventureTask : AccountTask
    {
        private readonly ITaskManager _taskManager;

        public StartAdventureTask(ITaskManager taskManager)
        {
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute(IServiceScope scoped, CancellationToken cancellationToken)
        {
            Result result;

            var toAdventurePageCommand = scoped.ServiceProvider.GetRequiredService<ToAdventurePageCommand>();
            result = await toAdventurePageCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var exploreAdventureCommand = scoped.ServiceProvider.GetRequiredService<ExploreAdventureCommand>();
            result = await exploreAdventureCommand.Execute(cancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var chromeBrowser = scoped.ServiceProvider.GetRequiredService<IDataService>().ChromeBrowser;
            var adventureDuration = AdventureParser.GetAdventureDuration(chromeBrowser.Html);
            await SetNextExecute(adventureDuration);

            return Result.Ok();
        }

        private async Task SetNextExecute(TimeSpan duration)
        {
            ExecuteAt = DateTime.Now.Add(duration * 2);
            await _taskManager.ReOrder(AccountId);
        }

        protected override string TaskName => "Start adventure";
    }
}