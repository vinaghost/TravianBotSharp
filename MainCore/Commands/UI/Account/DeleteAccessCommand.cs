using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.Account
{
    public class DeleteAccessCommand : IRequest
    {
        public DeleteAccessCommand(AccessInput selectedAccess, AccountInput accountInput)
        {
            SelectedAccess = selectedAccess;
            AccountInput = accountInput;
        }

        public AccessInput SelectedAccess { get; }
        public AccountInput AccountInput { get; }
    }

    public class DeleteAccessCommandHandler : IRequestHandler<DeleteAccessCommand>
    {
        public async Task Handle(DeleteAccessCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var accountInput = request.AccountInput;
            var selectedAccess = request.SelectedAccess;
            accountInput.Accesses.Remove(selectedAccess);
        }
    }
}