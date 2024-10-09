using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccessInputValidator : AbstractValidator<AccessInput>
    {
        public AccessInputValidator()
        {
            RuleFor(x => x.Username).NotEmpty();

            RuleFor(x => x.Password).NotEmpty();

            RuleFor(x => x.ProxyHost)
                .NotEmpty()
                .When(x => x.ProxyPort != 0)
                .WithName("Proxy's host");
            RuleFor(x => x.ProxyPort)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.ProxyHost))
                .WithName("Proxy's port");

            RuleFor(x => x.ProxyUsername)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.ProxyPassword))
                .WithName("Proxy's username");

            RuleFor(x => x.ProxyPassword)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.ProxyUsername))
                .WithName("Proxy's password");
        }
    }
}