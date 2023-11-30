using FluentValidation;
using MainCore.Services;
using MainCore.UI.Models.Input;
using MediatR;

namespace MainCore.Commands.UI.AddAccount
{
    public class EditAccessCommand : IRequest
    {
        public EditAccessCommand(AccessInput input, AccessInput target)
        {
            Input = input;
            Target = target;
        }

        public AccessInput Input { get; }
        public AccessInput Target { get; }
    }

    public class EditAccessCommandHandler : IRequestHandler<EditAccessCommand>
    {
        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IDialogService _dialogService;

        public EditAccessCommandHandler(IValidator<AccessInput> accessInputValidator, IDialogService dialogService)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;
        }

        public async Task Handle(EditAccessCommand request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            var input = request.Input;
            var target = request.Target;
            var result = _accessInputValidator.Validate(input);

            if (!result.IsValid)
            {
                _dialogService.ShowMessageBox("Error", result.ToString());
                return;
            }

            input.CopyTo(target);
        }
    }
}