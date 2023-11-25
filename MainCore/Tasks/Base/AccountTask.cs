using MainCore.Commands;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Tasks.Base
{
    public abstract class AccountTask : TaskBase
    {
        protected AccountTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        public AccountId AccountId { get; private set; }

        public void Setup(AccountId accountId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            CancellationToken = cancellationToken;
        }
    }
}