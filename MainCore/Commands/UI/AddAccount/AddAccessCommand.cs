using FluentValidation;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.AddAccount
{
    public class AddAccessCommand : IRequest
    {
        public AddAccessCommand(AccessInput accessInput, AccountInput accountInput)
        {
            AccessInput = accessInput;
            AccountInput = accountInput;
        }

        public AccessInput AccessInput { get; }
        public AccountInput AccountInput { get; }
    }

    public class AddAccessCommandHandler : IRequestHandler<AddAccessCommand>
    {
        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IDialogService _dialogService;

        public AddAccessCommandHandler(IValidator<AccessInput> accessInputValidator, IDialogService dialogService)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;
        }

        public async Task Handle(AddAccessCommand request, CancellationToken cancellationToken)
        {
            var accountInput = request.AccountInput;
            var accessInput = request.AccessInput;
            await Task.CompletedTask;
            var result = _accessInputValidator.Validate(accessInput);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            accountInput.Accesses.Add(accessInput.Clone());
        }
    }
}