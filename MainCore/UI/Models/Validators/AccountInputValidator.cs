using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccountInputValidator : AbstractValidator<AccountInput>
    {
        public AccountInputValidator()
        {
            RuleFor(x => x.Server)
                .NotEmpty()
                .WithName("Server url");

            RuleFor(x => x.Server)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _)).When(x => !string.IsNullOrEmpty(x.Server))
                .WithMessage("Invalid Server url, please follow the pattern [https://ts1.x1.international.travian.com]");

            RuleFor(x => x.Accesses)
                .NotEmpty()
                .WithName("Access list");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithName("Nick name");
        }
    }
}