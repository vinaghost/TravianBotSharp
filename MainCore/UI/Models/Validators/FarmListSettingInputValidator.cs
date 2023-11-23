using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class FarmListSettingInputValidator : AbstractValidator<FarmListSettingInput>
    {
        public FarmListSettingInputValidator()
        {
            RuleFor(x => x.FarmInterval.Min)
                .LessThanOrEqualTo(x => x.FarmInterval.Max)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be less than maximum farm interval ({ComparisonValue})");
            RuleFor(x => x.FarmInterval.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be positive number");
        }
    }
}