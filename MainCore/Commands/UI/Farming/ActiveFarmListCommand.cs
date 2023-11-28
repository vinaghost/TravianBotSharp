using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;

namespace MainCore.Commands.UI.Farming
{
    public class ActiveFarmListCommand : ByAccountIdBase, IRequest
    {
        public FarmId FarmId { get; }

        public ActiveFarmListCommand(AccountId accountId, FarmId farmId) : base(accountId)
        {
            FarmId = farmId;
        }
    }

    public class ActiveFarmListCommandHandler : IRequestHandler<ActiveFarmListCommand>
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public ActiveFarmListCommandHandler(IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(ActiveFarmListCommand request, CancellationToken cancellationToken)
        {
            var farmId = request.FarmId;
            var accountId = request.AccountId;
            _unitOfRepository.FarmRepository.ChangeActive(farmId);
            await _mediator.Publish(new FarmListUpdated(accountId));
        }
    }
}