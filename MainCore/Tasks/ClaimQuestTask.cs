using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Base;
using MainCore.Commands.Features;
using MainCore.Commands.Validate;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public class ClaimQuestTask : VillageTask
    {
        private readonly ICommandHandler<ValidateQuestCommand, bool> _validateQuestCommand;

        public ClaimQuestTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator, ICommandHandler<ValidateQuestCommand, bool> validateQuestCommand) : base(unitOfCommand, unitOfRepository, mediator)
        {
            _validateQuestCommand = validateQuestCommand;
        }

        protected override async Task<Result> Execute()
        {
            Result result;
            result = await _unitOfCommand.UpdateAccountInfoCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _validateQuestCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var isClaimable = _validateQuestCommand.Value;
            if (!isClaimable) return Result.Ok();

            result = await _mediator.Send(new ClaimQuestCommand(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            return Result.Ok();
        }

        protected override void SetName()
        {
            var village = _unitOfRepository.VillageRepository.GetVillageName(VillageId);
            _name = $"Claim quest in {village}";
        }
    }
}