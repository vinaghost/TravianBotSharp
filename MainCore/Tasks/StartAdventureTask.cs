using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features.Step.StartAdventure;
using MainCore.Commands.Navigate;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Parsers;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ICommandHandler<ToAdventurePageCommand> _toAdventurePageCommand;
        private readonly ICommandHandler<ExploreAdventureCommand> _exploreAdventureCommand;
        private readonly ITaskManager _taskManager;
        private readonly IChromeManager _chromeManager;
        private readonly UnitOfParser _unitOfParser;

        public StartAdventureTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ITaskManager taskManager, IChromeManager chromeManager, UnitOfParser unitOfParser, ICommandHandler<ToAdventurePageCommand> toAdventurePageCommand, ICommandHandler<ExploreAdventureCommand> exploreAdventureCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _taskManager = taskManager;
            _chromeManager = chromeManager;
            _unitOfParser = unitOfParser;
            _toAdventurePageCommand = toAdventurePageCommand;
            _exploreAdventureCommand = exploreAdventureCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1, true), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var chromeBrowser = _chromeManager.Get(AccountId);

            var html = chromeBrowser.Html;

            var canStartAdventure = _unitOfParser.HeroParser.CanStartAdventure(html);
            if (!canStartAdventure) return Result.Ok();

            result = await _toAdventurePageCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _exploreAdventureCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            html = chromeBrowser.Html;
            var adventureDuration = _unitOfParser.HeroParser.GetAdventureDuration(html);

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