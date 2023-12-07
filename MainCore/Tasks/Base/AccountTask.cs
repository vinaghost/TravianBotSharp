using FluentResults;
using MainCore.Commands;
using MainCore.Common.Errors;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        protected AccountTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        public AccountId AccountId { get; protected set; }

        public void Setup(AccountId accountId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            CancellationToken = cancellationToken;
        }

        protected override async Task<Result> PreExecute()
        {
            if (CancellationToken.IsCancellationRequested) return new Cancel();

            Result result;
            result = await _unitOfCommand.ValidateInGameCommand.Execute(AccountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var inGame = _unitOfCommand.ValidateInGameCommand.Value;

            if (inGame) return Result.Ok();

            result = await _unitOfCommand.ValidateLoginCommand.Execute(AccountId);
            if (result.IsFailed) return result.WithError(new TraceMessage(TraceMessage.Line()));

            var login = _unitOfCommand.ValidateLoginCommand.Value;

            if (login)
            {
                if (this is not LoginTask)
                {
                    ExecuteAt = ExecuteAt.AddMilliseconds(1975);
                    await _mediator.Publish(new AccountLogout(AccountId));
                    return Result.Fail(Skip.AccountLogout);
                }
                return Result.Ok();
            }

            return Result.Fail(Stop.TravianPage);
        }
    }
}