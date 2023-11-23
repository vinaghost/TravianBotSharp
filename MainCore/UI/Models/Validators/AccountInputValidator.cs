using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccountInputValidator : AbstractValidator<AccountInput>
    {
        public AccountInputValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty();
            RuleFor(x => x.Server)
                .NotEmpty()
                .WithName("Server url");
            RuleFor(x => x.Accesses)
                .NotEmpty()
                .WithName("Access list");
        }
    }
}