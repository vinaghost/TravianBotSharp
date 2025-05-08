using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class ResourceBuildInputValidator : AbstractValidator<ResourceBuildInput>
    {
        public ResourceBuildInputValidator()
        {
            RuleFor(x => x.Level)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}