using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class NormalBuildInputValidator : AbstractValidator<NormalBuildInput>
    {
        public NormalBuildInputValidator()
        {
            RuleFor(x => x.Level)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
