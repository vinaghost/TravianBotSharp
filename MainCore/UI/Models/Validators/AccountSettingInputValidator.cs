using FluentValidation;
using MainCore.UI.Models.Input;

namespace MainCore.UI.Models.Validators
{
    public class AccountSettingInputValidator : AbstractValidator<AccountSettingInput>
    {
        public AccountSettingInputValidator()
        {
            RuleFor(x => x.ClickDelay.Min)
                .LessThanOrEqualTo(x => x.ClickDelay.Max)
                .WithMessage("Minimum click delay ({PropertyValue}) should be less than maximum click delay ({ComparisonValue})");
            RuleFor(x => x.ClickDelay.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum click delay ({PropertyValue}) should be positive number");
            RuleFor(x => x.TaskDelay.Min)
                .LessThanOrEqualTo(x => x.TaskDelay.Max)
                .WithMessage("Minimum task delay ({PropertyValue}) should be less than maximum task delay ({ComparisonValue})");
            RuleFor(x => x.TaskDelay.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum task delay ({PropertyValue}) should be positive number");
            RuleFor(x => x.Tribe.SelectedItem.Tribe)
                .NotEqual(TribeEnums.Any)
                .WithMessage("Tribe should be specific");

            RuleFor(x => x.FarmInterval.Min)
                .LessThanOrEqualTo(x => x.FarmInterval.Max)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be less than maximum farm interval ({ComparisonValue})");
            RuleFor(x => x.FarmInterval.Min)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Minimum farm interval ({PropertyValue}) should be positive number");
        }
    }
}