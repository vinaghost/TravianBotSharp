using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features;
using MainCore.Commands.Features.Step.StartAdventure;
using MainCore.Commands.Validate;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class StartAdventureTask : AccountTask
    {
        private readonly ICommandHandler<ValidateAdventureCommand, bool> _validateAdventureCommand;
        private readonly ICommandHandler<GetDurationAdventureCommand, TimeSpan> _getDurationAdventureCommand;
        private readonly ITaskManager _taskManager;

        public StartAdventureTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ValidateAdventureCommand, bool> validateAdventureCommand, ICommandHandler<GetDurationAdventureCommand, TimeSpan> getDurationAdventureCommand, ITaskManager taskManager) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _validateAdventureCommand = validateAdventureCommand;
            _getDurationAdventureCommand = getDurationAdventureCommand;
            _taskManager = taskManager;
        }

        protected override async Task<Result> Execute()
        {
            Result result;

            result = await _unitOfCommand.ToDorfCommand.Handle(new(AccountId, 1, true), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _validateAdventureCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var canStartAdventure = _validateAdventureCommand.Value;
            if (!canStartAdventure) return Result.Ok();

            result = await _mediator.Send(new StartAdventureCommand(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            result = await _getDurationAdventureCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            await SetNextExecute(_getDurationAdventureCommand.Value);

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