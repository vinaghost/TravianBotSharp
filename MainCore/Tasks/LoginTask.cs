using FluentResults;
using MainCore.Commands;
using MainCore.Commands.Special;
using MainCore.Common.Errors;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Tasks.Base;
using MediatR;

namespace MainCore.Tasks
{
    [RegisterAsTransient(withoutInterface: true)]
    public sealed class LoginTask : AccountTask
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfCommand _unitOfCommand;

        public LoginTask(IMediator mediator, IUnitOfCommand unitOfCommand)
        {
            _mediator = mediator;
            _unitOfCommand = unitOfCommand;
        }

        public override async Task<Result> Execute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();
            Result result;
            result = await _mediator.Send(new LoginCommand(AccountId));
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            result = await _unitOfCommand.UpdateVillageListCommand.Execute(AccountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));
            return Result.Ok();
        }

        protected override void SetName()
        {
            _name = "Login task";
        }
    }
}