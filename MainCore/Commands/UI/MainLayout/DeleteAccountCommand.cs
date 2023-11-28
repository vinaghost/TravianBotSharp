using MainCore.Common.MediatR;
using MainCore.Entities;
using MainCore.Notification.Message;
using MainCore.Repositories;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.Commands.UI.MainLayout
{
    public class DeleteAccountCommand : ByAccountIdBase, IRequest
    {
        public DeleteAccountCommand(AccountId accountId) : base(accountId)
        {
        }
    }

    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        public DeleteAccountCommandHandler(IMediator mediator, IUnitOfRepository unitOfRepository)
        {
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;
        }

        public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var accountId = request.AccountId;
            await Observable.Start(() =>
            {
                _unitOfRepository.AccountRepository.Delete(accountId);
            }, RxApp.TaskpoolScheduler);
            await _mediator.Publish(new AccountUpdated(), cancellationToken);
        }
    }
}