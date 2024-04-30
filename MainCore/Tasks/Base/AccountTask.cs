namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        protected AccountTask(UnitOfCommand unitOfCommand, UnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
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
            if (CancellationToken.IsCancellationRequested) return Cancel.Error;

            Result result;
            result = await _unitOfCommand.ValidateIngameCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var inGame = _unitOfCommand.ValidateIngameCommand.Value;

            if (inGame) return Result.Ok();

            result = await _unitOfCommand.ValidateLoginCommand.Handle(new(AccountId), CancellationToken);
            if (result.IsFailed) return result.WithError(TraceMessage.Error(TraceMessage.Line()));

            var login = _unitOfCommand.ValidateLoginCommand.Value;

            if (login)
            {
                if (this is not LoginTask)
                {
                    ExecuteAt = ExecuteAt.AddMilliseconds(1975);
                    await _mediator.Publish(new AccountLogout(AccountId), CancellationToken);
                    return Result.Fail(Skip.AccountLogout);
                }
                return Result.Ok();
            }

            return Result.Fail(Stop.TravianPage);
        }
    }
}