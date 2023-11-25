using MainCore.Commands;
using MainCore.Entities;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Tasks.Base
{
    public abstract class VillageTask : AccountTask
    {
        protected VillageTask(IUnitOfCommand unitOfCommand, IUnitOfRepository unitOfRepository, IMediator mediator) : base(unitOfCommand, unitOfRepository, mediator)
        {
        }

        public VillageId VillageId { get; protected set; }

        public void Setup(AccountId accountId, VillageId villageId, CancellationToken cancellationToken = default)
        {
            AccountId = accountId;
            VillageId = villageId;
            CancellationToken = cancellationToken;
        }
    }
}